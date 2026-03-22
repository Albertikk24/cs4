using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TextFileEditor
{

    public class SearchResult : ISearchResult
    {

        public string FilePath { get; private set; }
        public Dictionary<string, int> Matches { get; private set; }
        public List<string> Errors { get; private set; }
        public int TotalMatches => Matches.Values.Sum();

        public SearchResult(string filePath)
        {
            FilePath = filePath;
            Matches = new Dictionary<string, int>();
            Errors = new List<string>();
        }

        public void AddMatch(string keyword, int count)
        {
            if (Matches.ContainsKey(keyword))
            {
                Matches[keyword] = Matches[keyword] + count;
            }
            else
            {
                Matches[keyword] = count;
            }
        }

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"File: {Path.GetFileName(FilePath)}");
            stringBuilder.AppendLine($"Path: {FilePath}");
            stringBuilder.AppendLine($"Total matches: {TotalMatches}");

            foreach (KeyValuePair<string, int> match in Matches)
            {
                stringBuilder.AppendLine($"  '{match.Key}': {match.Value} times");
            }

            if (Errors.Count > 0)
            {
                stringBuilder.AppendLine($"Errors: {Errors.Count}");
            }

            return stringBuilder.ToString();
        }
    }
}