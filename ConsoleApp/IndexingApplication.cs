using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TextFileEditor {

  public class IndexingApplication : IIndexingApplication {

    private FileIndex _currentIndex;

    public void Run() {
      string headerMessage = "==========================================\n" +
                           "     FILE INDEXING APPLICATION\n" +
                           "==========================================";
      Console.WriteLine(headerMessage);

      bool isRunning = true;

      while (isRunning) {
        string menu = "\n--- INDEXING MENU ---\n" +
                     "1. Index directory\n" +
                     "2. Search by keywords\n" +
                     "3. Load existing index\n" +
                     "4. View keyword statistics\n" +
                     "5. Exit\n" +
                     "Your choice: ";
        Console.Write(menu);

        string choice = Console.ReadLine();

        switch (choice) {
          case "1":
            IndexDirectory();
            break;

          case "2":
            SearchByKeywords();
            break;

          case "3":
            LoadIndex();
            break;

          case "4":
            ViewStatistics();
            break;

          case "5":
            isRunning = false;
            string goodbyeMessage = "\nGoodbye!";
            Console.WriteLine(goodbyeMessage);
            break;

          default:
            string invalidMessage = "Invalid choice. Please try again.";
            Console.WriteLine(invalidMessage);
            break;
        }
      }
    }

    private void IndexDirectory() {
      Console.Write("\nEnter directory to index: ");
      string directoryPath = Console.ReadLine();

      if (!Directory.Exists(directoryPath)) {
        string notFoundMessage = $"Directory not found: {directoryPath}";
        Console.WriteLine(notFoundMessage);
        return;
      }

      Console.Write("Search subdirectories? (yes/no): ");
      string subDirChoice = Console.ReadLine();
      bool searchSubdirectories = subDirChoice.ToLower() == "yes" || subDirChoice.ToLower() == "y";

      try {
        TextFileSearcher searcher = new TextFileSearcher(directoryPath, searchSubdirectories);

        Console.Write("Add custom file pattern? (yes/no): ");
        string addPatternChoice = Console.ReadLine();

        if (addPatternChoice.ToLower() == "yes" || addPatternChoice.ToLower() == "y") {
          Console.Write("Enter pattern (e.g., *.log): ");
          string pattern = Console.ReadLine();
          searcher.AddFilePattern(pattern);
        }

        string patterns = string.Join(", ", searcher.GetFilePatterns());
        Console.WriteLine($"\nSearching for patterns: {patterns}");

        Console.WriteLine("Indexing files... This may take a moment.");

        _currentIndex = searcher.CreateIndex();

        string successMessage = $"\nIndexing complete!\n" +
                               $"Found {_currentIndex.Files.Count} files\n" +
                               $"Found {_currentIndex.KeywordIndex.Count} unique keywords";
        Console.WriteLine(successMessage);

        SaveIndex(_currentIndex);

      } catch (Exception exception) {
        string errorMessage = $"Error during indexing: {exception.Message}";
        Console.WriteLine(errorMessage);
      }
    }

    private void SearchByKeywords() {
      if (_currentIndex == null) {
        string noIndexMessage = "No index loaded. Please index a directory first.";
        Console.WriteLine(noIndexMessage);
        return;
      }

      Console.Write("\nEnter keywords (comma-separated): ");
      string keywordInput = Console.ReadLine();

      List<string> keywords = keywordInput.Split(',')
                                          .Select(k => k.Trim())
                                          .Where(k => !string.IsNullOrWhiteSpace(k))
                                          .ToList();

      if (keywords.Count == 0) {
        string noKeywordsMessage = "No keywords entered.";
        Console.WriteLine(noKeywordsMessage);
        return;
      }

      Console.WriteLine($"\nSearching for: {string.Join(", ", keywords)}");

      Dictionary<string, List<string>> results = new Dictionary<string, List<string>>();

      foreach (string keyword in keywords) {
        List<string> files = _currentIndex.FindFilesByKeyword(keyword);
        if (files.Count > 0) {
          results[keyword] = files;
        }
      }

      if (results.Count == 0) {
        string noResultsMessage = "No files found containing the specified keywords.";
        Console.WriteLine(noResultsMessage);
        return;
      }

      StringBuilder resultsBuilder = new StringBuilder();
      resultsBuilder.AppendLine("\n=== SEARCH RESULTS ===");

      foreach (KeyValuePair<string, List<string>> result in results) {
        resultsBuilder.AppendLine($"\nKeyword: '{result.Key}' - found in {result.Value.Count} files:");
        foreach (string file in result.Value.Take(10)) {
          resultsBuilder.AppendLine($"  - {file}");
        }
        if (result.Value.Count > 10) {
          resultsBuilder.AppendLine($"  ... and {result.Value.Count - 10} more");
        }
      }

      Console.Write(resultsBuilder.ToString());
    }

    private void SaveIndex(FileIndex index) {
      try {
        string indexFilePath = Path.Combine(index.RootDirectory, "file_index.xml");

        XmlSerializer serializer = new XmlSerializer(typeof(FileIndex));
        using (StreamWriter writer = new StreamWriter(indexFilePath, false, Encoding.UTF8)) {
          serializer.Serialize(writer, index);
        }

        string saveMessage = $"Index saved to: {indexFilePath}";
        Console.WriteLine(saveMessage);
      } catch (Exception exception) {
        string errorMessage = $"Could not save index: {exception.Message}";
        Console.WriteLine(errorMessage);
      }
    }

    private void LoadIndex() {
      Console.Write("\nEnter index file path: ");
      string indexFilePath = Console.ReadLine();

      if (!File.Exists(indexFilePath)) {
        string notFoundMessage = $"File not found: {indexFilePath}";
        Console.WriteLine(notFoundMessage);
        return;
      }

      try {
        XmlSerializer serializer = new XmlSerializer(typeof(FileIndex));
        using (StreamReader reader = new StreamReader(indexFilePath, Encoding.UTF8)) {
          _currentIndex = (FileIndex)serializer.Deserialize(reader);
        }

        string loadMessage = $"Index loaded successfully!\n" +
                            $"{_currentIndex}";
        Console.WriteLine(loadMessage);
      } catch (Exception exception) {
        string errorMessage = $"Error loading index: {exception.Message}";
        Console.WriteLine(errorMessage);
      }
    }

    private void ViewStatistics() {
      if (_currentIndex == null) {
        string noIndexMessage = "No index loaded. Please index a directory first.";
        Console.WriteLine(noIndexMessage);
        return;
      }

      Dictionary<string, int> statistics = _currentIndex.GetKeywordStatistics();

      StringBuilder statisticsBuilder = new StringBuilder();
      statisticsBuilder.AppendLine($"\n=== KEYWORD STATISTICS ===");
      statisticsBuilder.AppendLine($"Total unique keywords: {statistics.Count}");
      statisticsBuilder.AppendLine($"\nTop 20 most common keywords:");

      int rank = 1;
      foreach (KeyValuePair<string, int> entry in statistics.Take(20)) {
        statisticsBuilder.AppendLine($"{rank,2}. '{entry.Key}' - appears in {entry.Value} files");
        rank++;
      }

      Console.Write(statisticsBuilder.ToString());
    }
  }
}