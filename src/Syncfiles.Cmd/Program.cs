using System;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.Extensions.CommandLineUtils;

namespace Syncfiles.Cmd
{
    class Program
    {
        public static void Main(string[] args)
        {
            //var result = ParseCommand(new [] { "scan", "-i",@"c:\my\New folder", "-o", @"c:\my\output"});
            var result = ParseCommand(args);
            Environment.Exit(result);
        }

        public static int ParseCommand(string[] args)
        {
            var res = ImageMetadataReader.ReadMetadata(@"C:\my\New folder\aparat 2016-07-29\DCIM\100CANON\IMG_6014.JPG")
                .ToList();
            var datetime = res.OfType<ExifIfd0Directory>().FirstOrDefault()?.GetDateTime(306);
            var app = new CommandLineApplication();

            app.Command("scan", config =>
            {
                config.Description = "Scan filesystem and suggest to move files to proper folders";
                var inputLocation = config.Option("-i | --input <DIR>",
                    "Input directory where the file scanning starts", CommandOptionType.SingleValue);
                var outputLocation = config.Option("-o | --output <DIR>",
                    "Output directory where the files should be moved", CommandOptionType.SingleValue);
                var reportFilename = config.Option("-r | --reportPath <DIR>", "Synchronization report file path",
                    CommandOptionType.SingleValue);
                ConfigureHelp(config);
                config.OnExecute(() =>
                {
                    if (inputLocation.HasValue() == false)
                    {
                        app.Out.WriteLine("You must specify input");
                        return 1;
                    }

                    if (outputLocation.HasValue() == false)
                    {
                        app.Out.WriteLine("You must specify output");
                        return 1;
                    }

                    var reportPath = reportFilename.HasValue()
                        ? reportFilename.Value()
                        : $"report_{DateTime.Now:yyyyMMddHHmmss}.txt";

                    Console.WriteLine("Report: " + reportPath);
                    var synchronizationService = new SynchronizationService();
                    var result =
                        synchronizationService.GenerateMoveFilesReport(inputLocation.Value(), outputLocation.Value());
                    System.IO.File.WriteAllLines(reportPath, result);
                    return 0;
                });
            });
            app.Command("move", config =>
            {
                config.Description = "Move files based on given report (report is generated with 'scan' option)";
                var reportFilename = config.Option("-r | --reportPath <DIR>", "Synchronization report file path",
                    CommandOptionType.SingleValue);
                ConfigureHelp(config);
                config.OnExecute(() =>
                {
                    if (reportFilename.HasValue() == false)
                    {
                        app.Out.WriteLine("You must specify reportFilename");
                        return 1;
                    }
                    var report = MoveFilesReport.Load(reportFilename.Value());
                    foreach (var item in report.Items)
                    {
                        Console.WriteLine($"{item.To}");
                    }

                    return 0;
                });
            });
            ConfigureHelp(app);

            return app.Execute(args);
        }

        private static void ConfigureHelp(CommandLineApplication config)
        {
            config.HelpOption("-? | -h | --help");
        }
    }
}