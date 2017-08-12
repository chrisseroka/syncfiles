using System.IO;
using System;
using Xunit;
using System.Globalization;
using System.Linq;
using Syncfiles.Cmd;

namespace Syncfiles.Tests
{
    public class SynchronizationServiceTests
    {
        private const string testMoveFileReportPath = "testMoveFileReportPath.txt";
        private const string testFilesDirectory = "syncFilesTestDir";
        private SynchronizationService service;
        private string outputFolder;
        private string inputFolder;

        public SynchronizationServiceTests()
        {
            this.inputFolder = PrepareFolder("input");
            this.outputFolder = PrepareFolder("output");

            this.service = new SynchronizationService();
        }

        [Fact]
        public void ShouldGenerateMoveFilesReport()
        {
            //ARRANGE
            TouchFile(inputFolder, @"download1.JPG", "2015-01-23");
            TouchFile(inputFolder, @"My vacations\DCIM\IMG_001.JPG", "2017-03-01");
            TouchFile(inputFolder, @"Personal\Unsorted\My Photos\2016_07_10.JPG", "2016-07-10");

            //ACT
            var report = this.service.GenerateMoveFilesReport(inputFolder, outputFolder);

            //ASSERT
            var expectedReport = new[]
            {
                ReportLine(inputFolder, @"download1.JPG", outputFolder, @"2015\01\download1.JPG"),
                ReportLine(inputFolder, @"My vacations\DCIM\IMG_001.JPG", outputFolder,
                    @"2017\03\My vacations\DCIM\IMG_001.JPG"),
                ReportLine(inputFolder, @"Personal\Unsorted\My Photos\2016_07_10.JPG", outputFolder,
                    @"2016\07\Personal\Unsorted\My Photos\2016_07_10.JPG"),
            };
            Assert.Equal(expectedReport[0], report[0]);
            Assert.Equal(expectedReport[1], report[1]);
            Assert.Equal(expectedReport[2], report[2]);
        }

        [Fact]
        public void ShouldReturnEmptyFileForEmptyInput()
        {
            //ARRANGE
            //Leaving empty...

            //ACT
            var report = this.service.GenerateMoveFilesReport(inputFolder, outputFolder);

            //ASSERT
            Assert.Equal(Array.Empty<string>(), report);
        }

        [Fact]
        public void ShouldReturnCorrectReportForSampleData()
        {
            this.PrepareMoveFilesReport(
                @"C:\folder1\file1.jpg   ///-->///   C:\2017\folder1\file1.jpg" + Environment.NewLine +
                @"C:\folder1\nestedfolder2\file2.jpg   ///-->///   C:\2017\folder1\nestedfolder2\file2.jpg"
            );
            var report = this.LoadMoveFilesReport();

            Assert.Equal(@"C:\folder1\file1.jpg", report.Items.ElementAt(0).From);
            Assert.Equal(@"C:\2017\folder1\file1.jpg", report.Items.ElementAt(0).To);

            Assert.Equal(@"C:\folder1\nestedfolder2\file2.jpg", report.Items.ElementAt(1).From);
            Assert.Equal(@"C:\2017\folder1\nestedfolder2\file2.jpg", report.Items.ElementAt(1).To);
        }

        [Fact]
        public void ShouldTrimSpaces()
        {
            this.PrepareMoveFilesReport(
                "     C:\\folder1\\file1.jpg   ///-->/// \t      C:\\2017\\folder1\\file1.jpg\t  \t"
            );
            var report = this.LoadMoveFilesReport();

            Assert.Equal(@"C:\folder1\file1.jpg", report.Items.ElementAt(0).From);
            Assert.Equal(@"C:\2017\folder1\file1.jpg", report.Items.ElementAt(0).To);
        }

        [Fact]
        public void ShouldNotRequireSpacesNextToSeparator()
        {
            this.PrepareMoveFilesReport(
                @"C:\folder1\file1.jpg///-->///C:\2017\folder1\file1.jpg"
            );
            var report = this.LoadMoveFilesReport();

            Assert.Equal(@"C:\folder1\file1.jpg", report.Items.ElementAt(0).From);
            Assert.Equal(@"C:\2017\folder1\file1.jpg", report.Items.ElementAt(0).To);
        }

        [Fact]
        public void ShouldAcceptSpacesInFileNames()
        {
            this.PrepareMoveFilesReport(
                @"C:\Program Files\some file with spaces .j pg///-->///C:\2017\Program Files\some file with spaces .j pg"
            );
            var report = this.LoadMoveFilesReport();

            Assert.Equal(@"C:\Program Files\some file with spaces .j pg", report.Items.ElementAt(0).From);
            Assert.Equal(@"C:\2017\Program Files\some file with spaces .j pg", report.Items.ElementAt(0).To);
        }

        [Fact]
        public void ShouldFailIfThereIsNoSeparator()
        {
            this.PrepareMoveFilesReport(
                @"C:\folder1\file1.jpg  C:\2017\folder1\file1.jpg"
            );

            var exception = Assert.Throws<InvalidOperationException>(() => this.LoadMoveFilesReport());
            Assert.Equal("Line 0: Separator not found", exception.Message);
        }

        [Fact]
        public void ShouldFailIfSeparatorIsIncorrect()
        {
            this.PrepareMoveFilesReport(
                @"C:\folder1\file1.jpg ///helloworld/// C:\2017\folder1\file1.jpg"
            );

            var exception = Assert.Throws<InvalidOperationException>(() => this.LoadMoveFilesReport());
            Assert.Equal("Line 0: Separator not found", exception.Message);
        }

        [Theory]
        [InlineData("///-->/// C:\\2017\\folder1\\file1.jpg", "Line 0: '' file is invalid")]
        [InlineData("C:\\folder1\\file1.jpg ///-->///", "Line 0: '' file is invalid")]
        [InlineData("asdf:\\folder1\\file1.jpg ///-->///", "Line 0: 'asdf:\\folder1\\file1.jpg' file is invalid")]
        [InlineData("asdf ///-->///", "Line 0: 'asdf' file is invalid")]
        public void ShouldFailIfThereIsNoFromFile(string line, string exceptionMessage)
        {
            this.PrepareMoveFilesReport(line);

            var exception = Assert.Throws<InvalidOperationException>(() => this.LoadMoveFilesReport());
            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public void ShouldScanMediaFiles()
        {
            //ARRANGE
            var files = new[]
            {
                "image1.jpg",
                "image2.3gp",
                "image3.mov",
                "image4.MOV",
                "image5.JPEG",
                "image6.mp4",
                "image6.avi",
                "image6.bmp",
            };
            foreach (var file in files)
            {
                TouchFile(inputFolder, file, "2015-01-23");
            }

            //ACT
            var report = this.service.GenerateMoveFilesReport(inputFolder, outputFolder);

            //ASSERT
            Assert.Equal(report.Length, files.Length);
        }

        [Fact]
        public void ShouldSkipOtherFiles()
        {
            //ARRANGE
            var files = new[]
            {
                ".ini",
                ".exe",
                ".mp3",
                ".jpg~",
                ".jpg.bak"
            };
            foreach (var file in files)
            {
                TouchFile(inputFolder, file, "2015-01-23");
            }

            //ACT
            var report = this.service.GenerateMoveFilesReport(inputFolder, outputFolder);

            //ASSERT
            Assert.Equal(Array.Empty<string>(), report);
        }

        [Fact]
        public void ShouldCountExceptionLineNumberCorrectly()
        {
            this.PrepareMoveFilesReport(
                @"C:\folder1\file1.jpg ///-->/// C:\2017\folder1\file1.jpg" + Environment.NewLine +
                @"C:\folder1\file1.jpg ///helloworld/// C:\2017\folder1\file1.jpg"
            );

            var exception = Assert.Throws<InvalidOperationException>(() => this.LoadMoveFilesReport());
            Assert.Equal("Line 1: Separator not found", exception.Message);
        }

        private void PrepareMoveFilesReport(string rawContent)
        {
            System.IO.File.WriteAllText(testMoveFileReportPath, rawContent);
        }

        private MoveFilesReport LoadMoveFilesReport()
        {
            return this.service.LoadMoveFilesReport(testMoveFileReportPath);
        }

        private string ReportLine(string inputFolder, string inputFile, string outputFolder, string outputFile)
        {
            var inputFilePath = Path.Combine(inputFolder, inputFile);
            var outputFilePath = Path.Combine(outputFolder, outputFile);
            return $"{inputFilePath}          ///-->///           {outputFilePath}";
        }

        private string PrepareFolder(string name)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), testFilesDirectory, name);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            return path;
        }

        private void TouchFile(string inputFolder, string relativePath, string creationDate)
        {
            var path = Path.Combine(inputFolder, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            File.Create(path).Dispose();
            File.SetCreationTime(path, DateTime.ParseExact(creationDate, "yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }
}