using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Syncfiles.Cmd
{
    public class MoveFilesReport
    {
        private static readonly char[] InvalidPathCharacters;

        static MoveFilesReport()
        {
            InvalidPathCharacters = Path.GetInvalidFileNameChars();
        }

        public IEnumerable<MoveFilesReportItem> Items { get; set; }

        public static MoveFilesReport Load(string path)
        {
            var result = new MoveFilesReport();
            var rawLines = File.ReadAllLines(path);
            var itemIndex = 0;
            result.Items = rawLines.Select(x => ReadLine(x, itemIndex++)).ToList();
            return result;
        }

        private static MoveFilesReportItem ReadLine(string rawLine, int lineIndex)
        {
            var parts = rawLine.Split(new[] {"-->"}, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                throw new InvalidOperationException($"Line {lineIndex}: Separator not found");
            }

            var from = Trim(parts[0]);
            var to = Trim(parts[1]);

            ValidateFile(from, lineIndex);
            ValidateFile(to, lineIndex);

            return new MoveFilesReportItem {From = from, To = to};
        }

        private static void ValidateFile(string path, int lineNumber)
        {
            var regex = "^[A-Za-z]:\\\\.+";
            if (Regex.IsMatch(path, regex) == false)
            {
                ThrowInvalidFileException(path, lineNumber);
            }

            var pathTokens = path.Split('\\', '/').Skip(1);
            if (pathTokens.Any(token => token.Any(character => InvalidPathCharacters.Contains(character))))
            {
                ThrowInvalidFileException(path, lineNumber);
            }
        }

        private static void ThrowInvalidFileException(string path, int lineNumber)
        {
            throw new InvalidOperationException($"Line {lineNumber}: '{path}' file is invalid");
        }

        private static string Trim(string token)
        {
            return token.Trim(' ', '/', '\t');
        }
    }
}