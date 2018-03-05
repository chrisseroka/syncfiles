using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace Syncfiles.Cmd2
{
    class Program
    {
        static void Main(string[] args)
        {
            var src = @"c:\my\New Folder";
            var dst = @"c:\my\syncfiles";
            var journal = System.IO.Path.Combine(src, "journal.csv");

            Console.WriteLine("Removing old file..");
            File.Delete(journal);

            Console.WriteLine("Scanning files...");
            var records = GetFileRecords(src, dst).ToList();
            Console.WriteLine("Processing result...");
            records = FindConflicts(FindDuplicates(records)).ToList();
            Console.WriteLine("Process copy or not...");
            records = CopyOrNot(records).ToList();

            Console.WriteLine("Exporting to excel...");
            ExportExcel(records, journal);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            System.Diagnostics.Process.Start("explorer", journal);
        }

        private static IEnumerable<FileRecord> CopyOrNot(IEnumerable<FileRecord> records)
        {
            var groupedByDestination = records.OrderByDescending(x => x.SourcePath).GroupBy(x => x.DestinationPath);
            foreach (var group in groupedByDestination)
            {
                var groupedByChecksum = group.GroupBy(x => x.Checksum);
                if (groupedByChecksum.Count() == 1)
                {
                    group.First().CopyOrNot = "copy";
                    foreach (var item in group.Skip(1))
                    {
                        item.CopyOrNot = "not";
                    }
                }
                else
                {
                    foreach (var item1 in group)
                    {
                        var firstInGroup = group.First();
                        var prefix = firstInGroup.RelativePath.ToLower().Replace("/", "-").Replace("\\", "-");
                        firstInGroup.DestinationPath = CalculateDestinationPath(firstInGroup.Year, firstInGroup.Month,
                            $"{prefix}-{firstInGroup.Filename}");
                        firstInGroup.CopyOrNot = "copy";
                        foreach (var item2 in group.Skip(1))
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
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            using (var writer = new System.IO.StreamWriter(fileStream))
            {
                writer.WriteLine("SourcePath\tFilename\tExtension\tChecksum\tDestinationPath\tDuplicatesCount\tYear\tMonth\tTimestamp\tRelativePath\tConflicts\tCopyOrNot\tSize");
                foreach (var line in records)
                {
                    writer.WriteLine($"{line.SourcePath}\t{line.Filename}\t{line.Extension}\t{line.Checksum}\t{line.DestinationPath}\t{line.DuplicatesCount}\t{line.Year}\t{line.Month}\t{line.Timestamp}\t{line.RelativePath}\t{line.Conflict}\t{line.CopyOrNot}\t{line.Size}");
                }
            }
        }

        public static IEnumerable<FileRecord> FindDuplicates(IEnumerable<FileRecord> records)
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

        public static IEnumerable<FileRecord> FindConflicts(IEnumerable<FileRecord> records)
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

        public static IEnumerable<FileRecord> GetFileRecords(string sourceDir, string destinationDir)
        {
            var files = System.IO.Directory.EnumerateFiles(sourceDir, "*", System.IO.SearchOption.AllDirectories);
            foreach (var fileName in files)
            {
                var creationDate = ReadDateTaken(fileName);
                var relativePath = fileName.Replace(sourceDir + "\\", "");
                var creationYear = creationDate.Year.ToString();
                var creationMonth = creationDate.Month.ToString("D2");
                var shortFileName = fileName.Split(new [] {"/", "\\"}.ToArray(), StringSplitOptions.RemoveEmptyEntries).Last();
                var outputFilepath = CalculateDestinationPath(creationYear, creationMonth, shortFileName);
                var fileInfo = new FileInfo(fileName);
                var checkSum = shortFileName.ToLower() + fileInfo.Length + creationDate;
                var record = new FileRecord
                {
                    Filename = shortFileName,
                    SourcePath = fileName,
                    Extension = shortFileName.Split('.').Last().ToLower(),
                    Year = creationYear,
                    Month = creationMonth,
                    Timestamp = creationDate,
                    DestinationPath = outputFilepath,
                    RelativePath = relativePath,
                    Checksum = checkSum,
                    Size = fileInfo.Length
                };
                yield return record;
            }
        }

        private static string CalculateDestinationPath(string year, string month, string fileName)
        {
            var outputFilepath = Path.Combine(year, month, fileName);
            return outputFilepath;
        }

        private static DateTime ReadDateTaken(string fileName)
        {
            if (fileName.ToLowerInvariant().EndsWith("jpg"))
            {
                var datetime = GetImageDateTakenProperty(fileName);
                if (datetime != null)
                {
                    return datetime.Value;
                }
            }
            return System.IO.File.GetLastWriteTime(fileName);
        }

        private static DateTime? GetImageDateTakenProperty(string path)
        {
            var metadata = ImageMetadataReader.ReadMetadata(path).ToList();
            DateTime result;
            if ((metadata.OfType<ExifIfd0Directory>().Any()
                 && metadata.OfType<ExifIfd0Directory>().FirstOrDefault().TryGetDateTime(306, out result))
                || (metadata.OfType<ExifSubIfdDirectory>().Any()
                    && metadata.OfType<ExifSubIfdDirectory>().First().TryGetDateTime(36867, out result)))
            {
                return result;
            }
            return null;
        }
    }

    public class FileRecord
    {
        public string Filename { get; set; }
        public string RelativePath { get; set; }
        public string SourcePath { get; set; }
        public string Checksum { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string DestinationPath { get; set; }
        public string Extension { get; set; }
        public int DuplicatesCount { get; set; }
        public string Conflict { get; set; }
        public string CopyOrNot { get; set; }
        public long Size { get; set; }
    }
}