using System;
using Xunit;

namespace Syncfiles.Tests
{
    public class WhenGettingMediaFileInfo
    {
    	private MediaFileInfo fileInfo;
    	private const string FilePath = ".\\files\\dir\\IMG_6220.JPG";

		public WhenGettingMediaFileInfo()
		{
			System.IO.File.SetCreationTime(FilePath, new DateTime(2017, 3, 11));
			this.fileInfo = new MediaFileInfo(FilePath);
		}

        [Fact]
        public void ShouldRetrieveFileHash()
        {
			Assert.Equal("12fe13c3351f2db4531a9c00755c9676", fileInfo.Hash);
        }

        [Fact]
        public void ShouldRetrieveFileName()
		{
			Assert.True(fileInfo.FilePath.EndsWith(FilePath));
		}

        [Fact]
        public void ShouldRetrieveCreationDate()
		{
			Assert.Equal(new DateTime(2017, 3, 11), fileInfo.CreationDate.Date);
		}
    }

}
