namespace Raven1_Worker
{
    public class MigrationInfo
    {
        public string SagaName { get; set; }
        public string DestinationServer { get; set; }
        public string SourceServer { get; set; }
        public string SourceDatabase { get; set; }
        public string DestinationDatabase { get; set; }
        public string EntityOldNamespace { get; set; }
        public string EntityNewNamespace { get; set; }
        public string NewRavenClrType { get; set; }
    }
}
