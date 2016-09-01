using System;
using CommandLine;
using CommandLine.Text;

namespace Raven1_Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Saga migrator started");
            var cmdArgs = new CommandLineOptions();
            if (Parser.Default.ParseArguments(args, cmdArgs))
            {
                var migrationInfo = new MigrationInfo()
                {
                    SourceDatabase = cmdArgs.SourceDatabase,
                    DestinationDatabase = cmdArgs.DestinationDatabase,
                    DestinationServer = cmdArgs.DestinationServer,
                    SourceServer = cmdArgs.SourceServer,
                    SagaName = cmdArgs.SagaName,
                    EntityOldNamespace = cmdArgs.EntityOldNamespace,
                    EntityNewNamespace = cmdArgs.EntityNewNamespace,
                    NewRavenClrType = cmdArgs.NewRavenClrType
                };
                var migrator = new SagaMigrator(migrationInfo);
                migrator.Run();
            }
        }

        class CommandLineOptions
        {
            [Option('t', "targetServer", Required = true,
              HelpText = "Target RavenDB server URL")]
            public string DestinationServer { get; set; }

            [Option('s', "sourceServer", Required = true,
              HelpText = "Source RavenDB server URL")]
            public string SourceServer { get; set; }

            [Option("targetDatabase", Required = true,
              HelpText = "Target database name")]
            public string DestinationDatabase { get; set; }

            [Option("sourceDatabase", Required = true,
              HelpText = "Source database name")]
            public string SourceDatabase { get; set; }

            [Option("sagaName", Required = true,
              HelpText = "Saga entity name")]
            public string SagaName { get; set; }

            [Option("oldNamespace",
              HelpText = "Old entity namespace")]
            public string EntityOldNamespace { get; set; }

            [Option("newNamespace",
              HelpText = "New entity namespace")]
            public string EntityNewNamespace { get; set; }

            [Option("newRavenClrType",
              HelpText = "New saga ravenClrType in metadata")]
            public string NewRavenClrType { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }
    }
}
