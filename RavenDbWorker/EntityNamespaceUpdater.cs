using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Json.Linq;

namespace RavenDbWorker
{
    /// <summary>
    /// Used to change namespaces in IDs of SagaUniqueIdentity collection
    /// </summary>
    public class EntityNamespaceUpdater
    {
        private readonly string _serverUrl;
        private readonly string _databaseName;

        public EntityNamespaceUpdater(string serverUrl, string databaseName)
        {
            _serverUrl = serverUrl;
            _databaseName = databaseName;
        }

        public void UpdateUniqueIdentityNamespace(string oldNamespace, string newNamespace)
        {
            using (IDocumentStore store = new DocumentStore()
            {
                Url = _serverUrl,
                DefaultDatabase = _databaseName
            }.Initialize())
            {
                using (IDocumentSession session = store.OpenSession())
                {
                    var documents = store.DatabaseCommands.StartsWith(oldNamespace, null, 0, 3);
                    while (documents.Length > 0)
                    {
                        Console.WriteLine("Updating {0} SagaUniqueIdentities", documents.Length);

                        foreach (var document in documents)
                        {
                            var oldKey = document.Key;
                            var updatedKey = document.Key.Replace(oldNamespace, newNamespace);

                            var newMetadata = CreateMetadata(document.Metadata);
                            document.Key = updatedKey;

                            store.DatabaseCommands.Put(updatedKey, null, document.DataAsJson, newMetadata);
                            store.DatabaseCommands.Delete(oldKey, null);
                        }

                        documents = store.DatabaseCommands.StartsWith(oldNamespace, null, 0, 1000);
                    }

                    Console.WriteLine("RavenDBWorker execution completed");
                }
            }
        }

        //Not used
        private RavenJObject UpdateSagaDocId(JsonDocument document, string oldKey, string updatedKey)
        {
            var json = document.DataAsJson;
            var valuesArray = json.Values().ToArray();
            var keysArray = json.Keys.ToArray();

            var newDocument = new RavenJObject();
            for (var i = 0; i < keysArray.Length; i++)
            {
                var keyName = keysArray[i];
                if (keyName == "SagaDocId")
                {
                    var newSagaDoc = new RavenJValue(valuesArray[i].ToString()
                        .Replace(oldKey.ToLower(), updatedKey.ToLower()));
                    valuesArray[i] = newSagaDoc;
                }

                newDocument.Add(keyName, valuesArray[i]);
            }
            
            return newDocument;
        }

        private RavenJObject CreateMetadata(RavenJObject oldMetadata)
        {
            var valuesArray = oldMetadata.Values().ToArray();
            var keysArray = oldMetadata.Keys.ToArray();

            var newMetadata = new RavenJObject();
            for(var i=0; i < keysArray.Length; i++)
            {
                var keyName = keysArray[i];
                if (keyName.StartsWith("@id") || keyName.StartsWith("@etag"))
                    continue;

                newMetadata.Add(keyName, valuesArray[i]);
            }

            return newMetadata;
        }
    }
}
