using System;
using Xunit;

namespace Syncfiles.Tests 
{
	public class WhenGettingReportFromFilesFolder 
	{
		public WhenGettingReportFromFilesFolder()
		{
			System.IO.File.SetCreationTime("files\\IMG_6153.JPG", new DateTime(2017, 1, 2).ToLocalTime());
			System.IO.File.SetCreationTime("files\\dir\\IMG_6220.JPG", new DateTime(2017, 1, 3).ToLocalTime());
		}

		[Fact]
		public void ShouldGenerateReportCorrectly()
		{
			var expectedReport = new Report(
						new ReportFile("files\\IMG_6153.JPG", "d6d2ea3e769584404d0905a4325d7e16", new DateTime(2017, 1, 2).ToLocalTime()),
						new ReportFolder("files\\dir",
							new ReportFile("files\\dir\\IMG_6220.JPG", "12fe13c3351f2db4531a9c00755c9676", new DateTime(2017, 1, 3).ToLocalTime())
							)
					);

			var report = FileScanner.Scan("files");

			Assert.Equal(expectedReport.ToJson(), report.ToJson());
		}
	}
}
