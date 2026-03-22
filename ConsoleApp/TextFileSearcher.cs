using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TextFileEditor {

  public class TextFileSearcher : ITextFileSearcher {

    private string _rootDirectory;
    private bool _searchSubdirectories;
    private List<string> _filePatterns;

    public TextFileSearcher(string rootDirectory, bool searchSubdirectories = true) {
      if (!Directory.Exists(rootDirectory)) {
        throw new DirectoryNotFoundException($"Directory not found: {rootDirectory}");
      }

      _rootDirectory = rootDirectory;
      _searchSubdirectories = searchSubdirectories;
      _filePatterns = new List<string> { "*.txt", "*.cs", "*.xml", "*.json", "*.html", "*.css", "*.js" };
    }

    public void AddFilePattern(string pattern) {
      if (!string.IsNullOrWhiteSpace(pattern)) {
        _filePatterns.Add(pattern);
      }
    }

    public List<string> GetFilePatterns() {
      return new List<string>(_filePatterns);
    }

    public List<SearchResult> SearchByKeywords(List<string> keywords, bool caseSensitive = false) {
      List<SearchResult> results = new List<SearchResult>();

      SearchOption searchOption = _searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

      foreach (string pattern in _filePatterns) {
        string[] files = Directory.GetFiles(_rootDirectory, pattern, searchOption);

        foreach (string file in files) {
          SearchResult result = SearchFile(file, keywords, caseSensitive);
          if (result.Matches.Count > 0) {
            results.Add(result);
          }
        }
      }

      return results.OrderByDescending(r => r.TotalMatches).ToList();
    }

    private SearchResult SearchFile(string filePath, List<string> keywords, bool caseSensitive) {
      SearchResult result = new SearchResult(filePath);

      try {
        string content = File.ReadAllText(filePath, Encoding.UTF8);
        string comparisonContent = caseSensitive ? content : content.ToLower();

        foreach (string keyword in keywords) {
          string comparisonKeyword = caseSensitive ? keyword : keyword.ToLower();

          int count = 0;
          int position = 0;
          while ((position = comparisonContent.IndexOf(comparisonKeyword, position, StringComparison.Ordinal)) != -1) {
            count++;
            position += comparisonKeyword.Length;
          }

          if (count > 0) {
            result.AddMatch(keyword, count);
          }
        }
      } catch (Exception exception) {
        result.AddError(exception.Message);
      }

      return result;
    }

    public FileIndex CreateIndex() {
      FileIndex index = new FileIndex(_rootDirectory);

      SearchOption searchOption = _searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

      foreach (string pattern in _filePatterns) {
        string[] files = Directory.GetFiles(_rootDirectory, pattern, searchOption);

        foreach (string file in files) {
          try {
            TextFile textFile = new TextFile(file);
            index.AddFile(textFile);
          } catch (Exception exception) {
            Console.WriteLine($"Warning: Could not index {file} - {exception.Message}");
          }
        }
      }

      return index;
    }
  }
}