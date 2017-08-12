using System;
using System.Collections.Generic;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace Syncfiles.Cmd
{
    public class SynchronizationService
    {
        public string[] GenerateMoveFilesReport(string inputFolder, string outputFolder)
        {
            var result = new List<string>();
            var files = System.IO.Directory.EnumerateFiles(inputFolder, "*", System.IO.SearchOption.AllDirectories);
            var outputFolderPath = new System.IO.DirectoryInfo(outputFolder).FullName;
            foreach (var fileName in files)
            {
                Console.WriteLine("path: " + fileName);
                var creationDate = ReadDateTaken(fileName);
                var relativePath = fileName.Replace(inputFolder + "\\", "");
                var creationYear = creationDate.Year.ToString();
                var creationMonth = creationDate.Month.ToString("D2");
                var outputFilepath = System.IO.Path.Combine(outputFolderPath, creationYear, creationMonth,
                    relativePath);
                var reportLine = BuildReportLine(fileName, outputFilepath);

                result.Add(reportLine);
            }

            return result.ToArray();
        }

        private DateTime ReadDateTaken(string fileName)
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

        private DateTime? GetImageDateTakenProperty(string path)
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

        public MoveFilesReport LoadMoveFilesReport(string path)
        {
            return MoveFilesReport.Load(path);
        }

        private string BuildReportLine(string inputFilePath, string outputFilePath)
        {
            return $"{inputFilePath}          ///-->///           {outputFilePath}";
        }
    }
}