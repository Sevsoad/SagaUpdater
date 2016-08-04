using System;
using CommandLine;
using CommandLine.Text;

namespace RavenDbWorker
{
    public class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("RavenDBWorker started");
            var cmdArgs = new CommandLineOptions();
            if (Parser.Default.ParseArguments(args, cmdArgs))
            {
                var dbWorker = new RavenDbWorker(cmdArgs.RavenServer, cmdArgs.DatabaseName);
                dbWorker.UpdateUniqueIdentityNamespace(cmdArgs.OldNamespace, cmdArgs.NewNamespace);
            }
        }
    }

    class CommandLineOptions
    {
        [Option('s', "server", Required = true,
          HelpText = "RavenDB server URL.")]
        public string RavenServer { get; set; }

        [Option('d', "database", Required = true,
          HelpText = "Database name to use in Raven.")]
        public string DatabaseName { get; set; }

        [Option('o', "oldNamespace", 
          HelpText = "Old namespace name.")]
        public string OldNamespace { get; set; }

        [Option('n', "newNamespace", 
          HelpText = "New namespace name.")]
        public string NewNamespace { get; set; }
        
        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
