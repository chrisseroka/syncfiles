using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Syncfiles.Cmd
{
    public static class FileRecordTools
    {
        public static IEnumerable<FileRecord> GetFileRecords(string sourceDir)
        {
            var files = System.IO.Directory.EnumerateFiles(sourceDir, "*", System.IO.SearchOption.AllDirectories);
            foreach (var fileName in files)
            {
                var creationDate = MediaFileTools.ReadDateTaken(fileName);
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
        public static string CalculateDestinationPath(string year, string month, string fileName)
        {
            var outputFilepath = Path.Combine(year, month, fileName);
            return outputFilepath;
        }
    }
}