using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Syncfiles
{
    public class MediaFileInfo 
	{
		public MediaFileInfo(string fileName)
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(fileName))
				{
					var hash = md5.ComputeHash(stream);
					StringBuilder sBuilder = new StringBuilder();

					// Loop through each byte of the hashed data 
					// and format each one as a hexadecimal string.
					for (int i = 0; i < hash.Length; i++)
					{
						sBuilder.Append(hash[i].ToString("x2"));
					}

					this.Hash = sBuilder.ToString();
				}
			}

			this.FilePath = fileName;
			this.CreationDate = new FileInfo(fileName).CreationTime;
		}

		public string Hash {get;set;}

		public string FilePath {get;set;}

		public DateTime CreationDate {get;set;}
	}
}
