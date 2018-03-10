using System;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;

namespace Syncfiles.Cmd
{
    public static class MediaFileTools
    {
        public static DateTime ReadDateTaken(string fileName)
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

        private static DateTime? GetImageDateTakenProperty(string path)
        {
            var metadata = ImageMetadataReader.ReadMetadata(path).ToList();
            DateTime result;
            if (metadata.OfType<IptcDirectory>().Any() && metadata.OfType<IptcDirectory>().First().TryGetDateTime(567, out result))
                return result;
            if (metadata.OfType<ExifSubIfdDirectory>().Any() && metadata.OfType<ExifSubIfdDirectory>().First().TryGetDateTime(36867, out result))
                return result;
            if (metadata.OfType<ExifIfd0Directory>().Any() && metadata.OfType<ExifIfd0Directory>().FirstOrDefault().TryGetDateTime(306, out result))
                return result;

            return null;
        }
    }
}