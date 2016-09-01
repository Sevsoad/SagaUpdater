using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Json.Linq;

namespace Raven1_Worker
{
    public class SagaMigrator
    {
        private MigrationInfo _migrationInfo;

        public SagaMigrator(MigrationInfo migrationInfo)
        {
            _migrationInfo = migrationInfo;
        }

        public void Run()
        {
            Console.WriteLine("Retrieveing saga files");
            var sagaFiles = GetSagaFiles(_migrationInfo);

            Console.WriteLine("Retrieveing uniqueIdentity files");
            var uniqueIdentities = GetUniqueIdentites(_migrationInfo);

            using (IDocumentStore store = new DocumentStore()
            {
                Url = _migrationInfo.DestinationServer,
                DefaultDatabase = _migrationInfo.DestinationDatabase
            }.Initialize())
            {
                using (IDocumentSession session = store.OpenSession())
                {
                    var updatedSagas = UpdateSagas(sagaFiles);
                    Console.WriteLine("Inserting {0} SagaFiles", sagaFiles.Count);
                    foreach (var file in updatedSagas)
                    {
                        store.DatabaseCommands.Put(file.Key, null, file.DataAsJson, file.Metadata);
                    }

                    var updatedIdentites = UpdateUniqueIdentites(uniqueIdentities);
                    Console.WriteLine("Inserting {0} UniqueIdentites", updatedIdentites.Count);
                    foreach (var file in updatedIdentites)
                    {
                        store.DatabaseCommands.Put(file.Key, null, file.DataAsJson, file.Metadata);
                    }
                }
            }
        }

        private List<JsonDocument> UpdateSagas(List<JsonDocument> sagas)
        {
            for (var i = 0; i < sagas.Count; i++)
            {
                var file = sagas[i];

                var metadata = CreateMetadata(file.Metadata);
                file.Metadata = UpdateRavenClrType(metadata);
            }

            return sagas;
        }

        private List<JsonDocument> UpdateUniqueIdentites(List<JsonDocument> idnetites)
        {
            for (var i = 0; i < idnetites.Count; i++)
            {
                var file = idnetites[i];
                
                var updatedKey = file.Key.Replace(_migrationInfo.EntityOldNamespace, _migrationInfo.EntityNewNamespace);
                file.Key = updatedKey;

                file.Metadata = CreateMetadata(file.Metadata);
            }

            return idnetites;
        }

        private List<JsonDocument> GetUniqueIdentites(MigrationInfo migrationInfo)
        {
            List<JsonDocument> uniqueIdentites = new List<JsonDocument>();

            using (IDocumentStore store = new DocumentStore()
            {
                Url = migrationInfo.SourceServer,
                DefaultDatabase = migrationInfo.SourceDatabase
            }.Initialize())
            {
                using (IDocumentSession session = store.OpenSession())
                {
                    var recordIndex = 0;
                    var stepSize = 400;
                    var recordsRead = 1;

                    while (recordsRead > 0)
                    {
                        var records = store.DatabaseCommands.StartsWith(migrationInfo.EntityOldNamespace, recordIndex, stepSize);
                        recordsRead = records.Length;
                        uniqueIdentites.AddRange(records);
                        Console.WriteLine("UniqueIdentity files summary retrieved: {0}", uniqueIdentites.Count);
                        recordIndex += stepSize;
                    }
                }
            }

            return uniqueIdentites;
        }

        private List<JsonDocument> GetSagaFiles(MigrationInfo migrationInfo)
        {
            List<JsonDocument> sagaFiles = new List<JsonDocument>();

            using (IDocumentStore store = new DocumentStore()
            {
                Url = migrationInfo.SourceServer,
                DefaultDatabase = migrationInfo.SourceDatabase
            }.Initialize())
            {
                using (IDocumentSession session = store.OpenSession())
                {
                    var recordIndex = 0;
                    var stepSize = 400;
                    var recordsRead = 1;

                    while (recordsRead > 0)
                    {
                        var records = store.DatabaseCommands.StartsWith(migrationInfo.SagaName, recordIndex, stepSize);
                        recordsRead = records.Length;
                        sagaFiles.AddRange(records);
                        Console.WriteLine("Saga files summary retrieved: {0}", sagaFiles.Count);
                        recordIndex += stepSize;
                    }
                }
            }

            return sagaFiles;
        }

        private RavenJObject CreateMetadata(RavenJObject oldMetadata)
        {
            var valuesArray = oldMetadata.Values().ToArray();
            var keysArray = oldMetadata.Keys.ToArray();

            var newMetadata = new RavenJObject();
            for (var i = 0; i < keysArray.Length; i++)
            {
                var keyName = keysArray[i];
                if (keyName.StartsWith("@id") || keyName.StartsWith("@etag"))
                    continue;

                newMetadata.Add(keyName, valuesArray[i]);
            }

            return newMetadata;
        }

        private RavenJObject UpdateRavenClrType(RavenJObject oldMetadata)
        {
            var valuesArray = oldMetadata.Values().ToArray();
            var keysArray = oldMetadata.Keys.ToArray();

            var newMetadata = new RavenJObject();
            for (var i = 0; i < keysArray.Length; i++)
            {
                var keyName = keysArray[i];
                if (keyName.StartsWith("Raven-Clr-Type"))
                {
                    newMetadata.Add(keyName, _migrationInfo.NewRavenClrType);
                    continue;
                }

                newMetadata.Add(keyName, valuesArray[i]);
            }

            return newMetadata;
        }
    }
}
