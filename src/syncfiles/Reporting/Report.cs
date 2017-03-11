using System;
using System.Text;

namespace Syncfiles
{
    public class Report 
	{
		public IReportItem[] Items {get;}

		public Report(params IReportItem[] items) 
		{
			this.Items = items;
		}

		public string ToText()
		{
			var sb = new StringBuilder();
			foreach(var item in this.Items)
			{
				this.ToText(sb, item, 0);
			}
			return sb.ToString();
		}

		private void ToText(StringBuilder sb, IReportItem item, int level)
		{
			var tab = new String(' ', level * 2);
			if (item is ReportFile)
			{
				var reportFile = (ReportFile)item;
				sb.AppendLine($"{reportFile.Name}, #{reportFile.Hash}");
			}
			foreach(var childItem in this.Items)
			{

			}

		}

		public string ToJson()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(this);
		}
	}
}
