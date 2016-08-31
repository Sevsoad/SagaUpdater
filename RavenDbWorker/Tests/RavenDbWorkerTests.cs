using NUnit.Framework;
using NUnit.Framework.Internal;
using Raven.Json.Linq;

namespace RavenDbWorker.Tests
{
    [TestFixture]
    public class RavenDbWorkerTests
    {
        private string _server = "http://localhost:8080";
        private string _database = "Host1";

        [Test]
        public void SaveCorrectMeatadata()
        {
            var worker = new EntityNamespaceUpdater(_server, _database);

            worker.UpdateUniqueIdentityNamespace("oldNamespace", "newNamespace");

        }
        
    }
}
