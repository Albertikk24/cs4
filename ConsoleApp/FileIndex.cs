using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TextFileEditor {

  public class FileIndex : IFileIndex {

    public string RootDirectory { get; private set; }
    public DateTime IndexedAt { get; private set; }
    public List<TextFile> Files { get; private set; }
    public Dictionary<string, List<string>> KeywordIndex { get; private set; }

    public FileIndex(string rootDirectory) {
      RootDirectory = rootDirectory;
      IndexedAt = DateTime.Now;
      Files = new List<TextFile>();
      KeywordIndex = new Dictionary<string, List<string>>();
    }

    public void AddFile(TextFile textFile) {
      Files.Add(textFile);

      foreach (KeyValuePair<string, int> wordFrequency in textFile.WordFrequency) {
        string keyword = wordFrequency.Key;

        if (!KeywordIndex.ContainsKey(keyword)) {
          KeywordIndex[keyword] = new List<string>();
        }

        if (!KeywordIndex[keyword].Contains(textFile.FilePath)) {
          KeywordIndex[keyword].Add(textFile.FilePath);
        }
      }
    }

    public List<string> FindFilesByKeyword(string keyword) {
      string searchKeyword = keyword.ToLower();

      if (KeywordIndex.ContainsKey(searchKeyword)) {
        return new List<string>(KeywordIndex[searchKeyword]);
      }

      return new List<string>();
    }

    public Dictionary<string, int> GetKeywordStatistics() {
      Dictionary<string, int> statistics = new Dictionary<string, int>();

      foreach (KeyValuePair<string, List<string>> keywordEntry in KeywordIndex) {
        statistics[keywordEntry.Key] = keywordEntry.Value.Count;
      }

      return statistics.OrderByDescending(entry => entry.Value)
                      .ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    public override string ToString() {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine($"=== FILE INDEX ===");
      stringBuilder.AppendLine($"Root: {RootDirectory}");
      stringBuilder.AppendLine($"Indexed: {IndexedAt}");
      stringBuilder.AppendLine($"Files: {Files.Count}");
      stringBuilder.AppendLine($"Unique keywords: {KeywordIndex.Count}");
      stringBuilder.AppendLine("==================");

      return stringBuilder.ToString();
    }
  }
}