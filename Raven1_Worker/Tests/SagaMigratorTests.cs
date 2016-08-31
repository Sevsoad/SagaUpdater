using NUnit.Framework;

namespace Raven1_Worker.Tests
{
    [TestFixture]
    public class SagaMigratorTests
    {
        private MigrationInfo _migrationInfo;

        [SetUp]
        public void SetUp()
        {
            _migrationInfo = new MigrationInfo
            {
                DestinationServer = "http://localhost:8080",
                DestinationDatabase = "Host1",
                SourceDatabase = "NewHost2",
                SourceServer = "http://localhost:8080",
                SagaName = "TestSaga",
                EntityOldNamespace = "Handlers.Messages.Sagas",
                EntityNewNamespace = "Handlers.Messages.NewSagas",
                NewRavenClrType = "Handlers.Messages.NewSagas.TestSaga, Handlers"
            };
        }

        [Test]
        public void Run()
        {
            var migrator = new SagaMigrator(_migrationInfo);

            migrator.Run();

        }

    }
}
