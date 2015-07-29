using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TwoWayMapper.Engine
{
    public static class ExpressionPathStringHelper
    {
        private static readonly Regex PathIndexRegex = new Regex(@"\[(\d*)\]", RegexOptions.Compiled | RegexOptions.Singleline);

        public static string EmptyPathIndexer(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            return PathIndexRegex.Replace(path, "[]");
        }

        public static int[] ExtractIndexes(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            return PathIndexRegex.Matches(path)
                                 .Cast<Match>()
                                 .Select(m => m.Groups[1].Value)
                                 .Select(v =>
                                 {
                                     return Int32.Parse(v);
                                 })
                                 .ToArray();
        }
    }
}