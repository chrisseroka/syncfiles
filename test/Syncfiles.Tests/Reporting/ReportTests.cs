using System;
using Xunit;

namespace Syncfiles.Tests.Reporting
{
    public class ReportTests 
	{
		[Fact]
		public void WhenFlatFileStructure_ShouldDumpReportToText()
		{
			var expectedReport = new Report(
						new ReportFile("IMG_6153.JPG", "hash1", new DateTime(2017, 1, 2)),
						new ReportFile("image.png", "hash2", new DateTime(2017, 1, 3))
					);

			var result = expectedReport.ToText();

			Assert.Equal("IMG_6153.JPG, #hash1" + Environment.NewLine +
					     "image.png, #hash2" + Environment.NewLine, result);
		}
	}
}
