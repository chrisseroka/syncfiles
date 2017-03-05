using System;

namespace Syncfiles
{
	public class ReportFolder: IReportItem
	{
		public IReportItem[] Items {get;}

		public ReportFolder(string name, params IReportItem[] items)
		{
			this.Items = items;
		}

	}
}
