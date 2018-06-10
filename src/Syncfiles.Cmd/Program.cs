using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace Syncfiles.Cmd
{
    public static class Program
    {
        // ReSharper disable once RedundantAssignment
        public static void Main(string[] args)
        {
            //args = new[] {"copy", "-s", @"c:\my\New Folder", "-o", @"d:\sorted", "-r", "journal.csv"};
            //args = new[] {"copy", "--nocopy", "-s", @"c:\my\New Folder", "-o", @"d:\sorted", "-r", "journal.csv"};
            //args = new[] {"getdate", "-f", @"C:\my\syncfiles\2016\12\02_IMG_5679.JPG"};
            var app = new CommandLineApplication();
            app.Command("copy", config =>
            {
                config.Description = "Scan source directory, generate CSV file and copy files to destination directory";
                var src = config.Option("-s | --src <DIR>",
                    "Source directory for file scanning", CommandOptionType.SingleValue);
                var dest = config.Option("-o | --output <DIR>",
                    "Output directory where the files should be moved", CommandOptionType.SingleValue);
                var reportFilename = config.Option("-r | --reportPath <DIR>", "Synchronization report file path",
                    CommandOptionType.SingleValue);
                var simulate = config.Option("-n | --nocopy", "simulate only (DO NOT COPY FILES)",
                    CommandOptionType.NoValue);

                ConfigureHelp(config);
                config.OnExecute(() =>
                {
                    if (src.HasValue() == false)
                    {
                        app.Out.WriteLine("You must specify input");
                        return 1;
                    }

                    if (dest.HasValue() == false)
                    {
                        app.Out.WriteLine("You must specify output");
                        return 1;
                    }

                    var reportPath = reportFilename.HasValue()
                        ? reportFilename.Value()
                        : $"journal.csv";

                    CopyFilesFromSrcToDst(src.Value(), dest.Value(), reportPath, simulate.HasValue());
                    return 0;
                });
            });
            app.Command("getdate", config =>
            {
                config.Description = "Get date for given image";
                var src = config.Option("-f | --file <FILE>",
                    "Input file name", CommandOptionType.SingleValue);

                ConfigureHelp(config);
                config.OnExecute(() =>
                {
                    if (src.HasValue() == false)
                    {
                        app.Out.WriteLine("You must specify input");
                        return 1;
                    }

                    DebugPath(src.Value());
                    return 0;
                });
            });
            ConfigureHelp(app);

            var appResult = app.Execute(args);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Environment.Exit(appResult);
        }

        private static void ConfigureHelp(CommandLineApplication config)
        {
            config.HelpOption("-? | -h | --help");
        }

        private static void DebugPath(string path)
        {
            var date = MediaFileTools.ReadDateTaken(path);
            Console.WriteLine("Calculated date is: " + date);
        }

        private static void GenerateBasicStats(IList<FileRecord> records)
        {
            var allCount = records.Count;
            Console.WriteLine($"Total files count: {allCount}");
            var uniqueCount = records.GroupBy(x => x.Checksum).Count();
            Console.WriteLine($"Unique files count: {uniqueCount}");
            Console.WriteLine($"Duplicates count: {allCount-uniqueCount}");
            foreach (var group in records.GroupBy(x => x.Extension.ToLower()))
            {
                var groupUniqueCount = group.GroupBy(x => x.Checksum).Count();
                Console.WriteLine($"{group.Key}: {group.Count()}, unique: {groupUniqueCount}, duplicates: {group.Count() - groupUniqueCount}");
            }
        }

        private static IList<FileRecord> GetRecords(string src, string journal)
        {
            Console.WriteLine("Scanning files...");
            var records = FileRecordTools.GetFileRecords(src).ToList();
            Console.WriteLine("Processing result...");
            records = FindConflicts(FindDuplicates(records).ToList()).ToList();
            Console.WriteLine("Process copy or not...");
            records = CopyOrNot(records).ToList();

            Console.WriteLine("Exporting to excel...");
            ExportExcel(records, journal);

            Console.WriteLine("Generate stats for source directory");
            GenerateBasicStats(records);
            return records;
        }

        static void CopyFilesFromSrcToDst(string src, string dst, string journalFilename, bool simulate)
        {
            var journal = Path.Combine(dst, journalFilename);

            Console.WriteLine("Removing old journal file..");
            if (File.Exists(journal))
            {
                File.Delete(journal);
            }

            var records = GetRecords(src, journal);
            if (simulate == false)
            {
                Console.WriteLine("Copying files");
                CopyFiles(records, dst);

                Console.WriteLine("Generate stats for destination directory");
                var recordsDst = FileRecordTools.GetFileRecords(dst).ToList();
                GenerateBasicStats(recordsDst);
            }
            System.Diagnostics.Process.Start("explorer", journal);
        }


        private static void CopyFiles(IList<FileRecord> records, string destination)
        {
            var toCopy = records.Where(x => x.CopyOrNot == "copy").ToList();
            var length = toCopy.Count;
            var index = 0;
            var time = DateTime.Now;
            foreach(var item in toCopy)
            {
                if (DateTime.Now > time.AddSeconds(1))
                {
                    Console.WriteLine($"Copying files... [{index*100/length}%]: {item.SourcePath}");
                    time = DateTime.Now;
                }
                CopyFile(item.SourcePath, Path.Combine(destination, item.DestinationPath));
                index++;
            }
        }

        private static void CreatePath(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void CopyFile(string src, string dst)
        {
            CreatePath(dst);
            File.Copy(src, dst);
        }

        private static IEnumerable<FileRecord> CopyOrNot(IList<FileRecord> records)
        {
            var groupedByDestination = records.OrderByDescending(x => x.SourcePath).GroupBy(x => x.DestinationPath);
            foreach (var group in groupedByDestination)
            {
                var groupedByChecksum = group.GroupBy(x => x.Checksum).ToList();
                if (groupedByChecksum.Count == 1)
                {
                    group.First().CopyOrNot = "copy";
                    foreach (var item in group.Skip(1))
                    {
                        item.CopyOrNot = "not";
                    }
                }
                else
                {
                    foreach (var item1 in groupedByChecksum)
                    {
                        var firstInGroup = item1.First();
                        var prefix = firstInGroup.RelativePath.ToLower().Replace("/", "-").Replace("\\", "-");
                        firstInGroup.DestinationPath = FileRecordTools.CalculateDestinationPath(firstInGroup.Year, firstInGroup.Month,
                            $"{prefix}-{firstInGroup.Filename}");
                        firstInGroup.CopyOrNot = "copy";
                        foreach (var item2 in item1.Skip(1))
                        {
                            item2.CopyOrNot = "not";
                        }
                    }
                }
            }
            return records;
        }

        private static void ExportExcel(IEnumerable<FileRecord> records, string fileName)
        {
            CreatePath(fileName);
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            using (var writer = new StreamWriter(fileStream))
            {
                writer.WriteLine("SourcePath\tFilename\tExtension\tChecksum\tDestinationPath\tDuplicatesCount\tYear\tMonth\tTimestamp\tRelativePath\tConflicts\tCopyOrNot\tSize");
                foreach (var line in records)
                {
                    writer.WriteLine($"{line.SourcePath}\t{line.Filename}\t{line.Extension}\t{line.Checksum}\t{line.DestinationPath}\t{line.DuplicatesCount}\t{line.Year}\t{line.Month}\t{line.Timestamp}\t{line.RelativePath}\t{line.Conflict}\t{line.CopyOrNot}\t{line.Size}");
                }
            }
        }

        private static IEnumerable<FileRecord> FindDuplicates(IEnumerable<FileRecord> records)
        {
            var groupedByChecksum = records.GroupBy(x => x.Checksum);
            foreach (var group in groupedByChecksum)
            {
                var count = group.Count();
                foreach (var item in group)
                {
                    item.DuplicatesCount = count - 1;
                    yield return item;
                }
            }
        }

        private static IEnumerable<FileRecord> FindConflicts(IEnumerable<FileRecord> records)
        {
            var groupedByDestination = records.GroupBy(x => x.DestinationPath);
            foreach (var group in groupedByDestination)
            {
                var count = group.Count();
                foreach (var item in group)
                {
                    item.Conflict = (count - 1).ToString();
                    yield return item;
                }
            }
        }
    }
}
