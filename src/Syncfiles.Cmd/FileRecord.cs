using System;

namespace Syncfiles.Cmd
{
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