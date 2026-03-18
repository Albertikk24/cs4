using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace TextFileEditor {

  // Custom exceptions
  public class TextFileException : Exception {

    public TextFileException(string message) : base(message) { }

  }

  public class FileNotFoundException : TextFileException {

    public FileNotFoundException(string filePath) : base($"File not found: {filePath}") { }

  }

  // Class representing a text file with serialization capabilities
  [Serializable]
  public class TextFile {

    public string FilePath { get; set; }
    public string Content { get; set; }
    public DateTime LastModified { get; set; }
    public Dictionary<string, int> WordFrequency { get; set; }

    // Default constructor for serialization
    public TextFile() {
      FilePath = string.Empty;
      Content = string.Empty;
      LastModified = DateTime.Now;
      WordFrequency = new Dictionary<string, int>();
    }

    public TextFile(string filePath) {
      if (string.IsNullOrWhiteSpace(filePath)) {
        throw new ArgumentException("File path cannot be empty.");
      }

      FilePath = filePath;
      LastModified = DateTime.Now;
      WordFrequency = new Dictionary<string, int>();

      if (File.Exists(filePath)) {
        LoadFromFile();
      } else {
        Content = string.Empty;
      }
    }

    public TextFile(string filePath, string content) : this(filePath) {
      Content = content;
      UpdateWordFrequency();
    }

    // Load content from physical file
    public void LoadFromFile() {
      if (!File.Exists(FilePath)) {
        throw new FileNotFoundException(FilePath);
      }

      Content = File.ReadAllText(FilePath, Encoding.UTF8);
      LastModified = File.GetLastWriteTime(FilePath);
      UpdateWordFrequency();
    }

    // Save content to physical file
    public void SaveToFile() {
      string directory = Path.GetDirectoryName(FilePath);
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
        Directory.CreateDirectory(directory);
      }

      File.WriteAllText(FilePath, Content, Encoding.UTF8);
      LastModified = DateTime.Now;
    }

    // Update word frequency dictionary
    public void UpdateWordFrequency() {
      WordFrequency.Clear();

      if (string.IsNullOrWhiteSpace(Content)) {
        return;
      }

      // Split into words (simple approach)
      char[] separators = { ' ', '\n', '\r', '\t', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '"', '\'' };
      string[] words = Content.Split(separators, StringSplitOptions.RemoveEmptyEntries);

      foreach (string word in words) {
        string cleanWord = word.ToLower();
        if (WordFrequency.ContainsKey(cleanWord)) {
          WordFrequency[cleanWord] = WordFrequency[cleanWord] + 1;
        } else {
          WordFrequency[cleanWord] = 1;
        }
      }
    }

    // JSON serialization using System.Text.Json
    public void JsonSerialize(string targetFilePath = null) {
      string filePath = targetFilePath ?? FilePath + ".json";

      JsonSerializerOptions options = new JsonSerializerOptions {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
      };
      
      string json = JsonSerializer.Serialize(this, options);
      File.WriteAllText(filePath, json, Encoding.UTF8);
    }

    // JSON deserialization using System.Text.Json
    public static TextFile JsonDeserialize(string filePath) {
      if (!File.Exists(filePath)) {
        throw new FileNotFoundException(filePath);
      }

      string json = File.ReadAllText(filePath, Encoding.UTF8);
      JsonSerializerOptions options = new JsonSerializerOptions {
        PropertyNameCaseInsensitive = true
      };
      
      return JsonSerializer.Deserialize<TextFile>(json, options);
    }

    // XML serialization
    public void XmlSerialize(string targetFilePath = null) {
      string filePath = targetFilePath ?? FilePath + ".xml";

      XmlSerializer serializer = new XmlSerializer(typeof(TextFile));
      using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8)) {
        serializer.Serialize(writer, this);
      }
    }

    // XML deserialization
    public static TextFile XmlDeserialize(string filePath) {
      if (!File.Exists(filePath)) {
        throw new FileNotFoundException(filePath);
      }

      XmlSerializer serializer = new XmlSerializer(typeof(TextFile));
      using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8)) {
        return (TextFile)serializer.Deserialize(reader);
      }
    }

    public override string ToString() {
      return $"File: {Path.GetFileName(FilePath)}\n" +
             $"Path: {FilePath}\n" +
             $"Last Modified: {LastModified}\n" +
             $"Size: {Content.Length} characters\n" +
             $"Words: {WordFrequency.Count} unique\n" +
             $"Content Preview: {(Content.Length > 100 ? Content.Substring(0, 100) + "..." : Content)}";
    }

  }

  // Memento class for undo/redo functionality
  public class TextFileMemento {

    public string Content { get; private set; }
    public DateTime Timestamp { get; private set; }

    public TextFileMemento(string content) {
      Content = content;
      Timestamp = DateTime.Now;
    }

  }

  // Originator class that creates and restores mementos
  public class TextFileOriginator {

    private TextFile _textFile;

    public TextFileOriginator(TextFile textFile) {
      _textFile = textFile ?? throw new ArgumentNullException(nameof(textFile));
    }

    public TextFileMemento SaveState() {
      return new TextFileMemento(_textFile.Content);
    }

    public void RestoreState(TextFileMemento memento) {
      if (memento == null) {
        throw new ArgumentNullException(nameof(memento));
      }

      _textFile.Content = memento.Content;
      _textFile.LastModified = DateTime.Now;
      _textFile.UpdateWordFrequency();
    }
  }

  // Caretaker class for managing undo/redo history
  public class TextFileHistory {

    private List<TextFileMemento> _undoStack = new List<TextFileMemento>();
    private List<TextFileMemento> _redoStack = new List<TextFileMemento>();
    private TextFileOriginator _originator;
    private int _maxHistorySize;
    private TextFileMemento _initialState;

    public bool CanUndo => _undoStack.Count > 1; // Keep at least initial state
    public bool CanRedo => _redoStack.Count > 0;

    public TextFileHistory(TextFile textFile, int maxHistorySize = 50) {
      _originator = new TextFileOriginator(textFile);
      _maxHistorySize = maxHistorySize;
      
      // Save initial state
      _initialState = _originator.SaveState();
      _undoStack.Add(_initialState);
    }

    public void SaveState() {
      TextFileMemento memento = _originator.SaveState();
      _undoStack.Add(memento);
      
      // Limit history size
      if (_undoStack.Count > _maxHistorySize) {
        _undoStack.RemoveAt(0);
      }
      
      // Clear redo stack when new changes are made
      _redoStack.Clear();
    }

    public void Undo() {
      if (!CanUndo) {
        // If only initial state exists, restore it
        if (_undoStack.Count == 1) {
          _originator.RestoreState(_initialState);
        }
        return;
      }

      // Move current state to redo stack
      TextFileMemento currentState = _originator.SaveState();
      _redoStack.Add(currentState);

      // Remove current state from undo stack
      _undoStack.RemoveAt(_undoStack.Count - 1);

      // Restore previous state
      _originator.RestoreState(_undoStack[_undoStack.Count - 1]);
    }

    public void Redo() {
      if (!CanRedo) {
        return;
      }

      TextFileMemento redoState = _redoStack[_redoStack.Count - 1];
      _redoStack.RemoveAt(_redoStack.Count - 1);

      _originator.RestoreState(redoState);
      _undoStack.Add(redoState);
    }

    public void ResetToInitialState() {
      _originator.RestoreState(_initialState);
      _undoStack.Clear();
      _redoStack.Clear();
      _undoStack.Add(_initialState);
    }

  }

  // Class for searching text files by keywords
  public class TextFileSearcher {

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

    // Search for files containing keywords
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

      // Sort by relevance (match count)
      return results.OrderByDescending(r => r.TotalMatches).ToList();
    }

    // Search single file for keywords
    private SearchResult SearchFile(string filePath, List<string> keywords, bool caseSensitive) {
      SearchResult result = new SearchResult(filePath);

      try {
        string content = File.ReadAllText(filePath, Encoding.UTF8);
        string comparisonContent = caseSensitive ? content : content.ToLower();

        foreach (string keyword in keywords) {
          string comparisonKeyword = caseSensitive ? keyword : keyword.ToLower();
          
          // Count occurrences
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
        // Skip files that can't be read
        result.AddError(exception.Message);
      }

      return result;
    }

    // Index files in directory by keywords
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
            // Skip files that can't be read
            Console.WriteLine($"Warning: Could not index {file} - {exception.Message}");
          }
        }
      }

      return index;
    }

  }

  // Search result class
  public class SearchResult {

    public string FilePath { get; private set; }
    public Dictionary<string, int> Matches { get; private set; }
    public List<string> Errors { get; private set; }
    public int TotalMatches => Matches.Values.Sum();

    public SearchResult(string filePath) {
      FilePath = filePath;
      Matches = new Dictionary<string, int>();
      Errors = new List<string>();
    }

    public void AddMatch(string keyword, int count) {
      if (Matches.ContainsKey(keyword)) {
        Matches[keyword] = Matches[keyword] + count;
      } else {
        Matches[keyword] = count;
      }
    }

    public void AddError(string error) {
      Errors.Add(error);
    }

    public override string ToString() {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine($"File: {Path.GetFileName(FilePath)}");
      stringBuilder.AppendLine($"Path: {FilePath}");
      stringBuilder.AppendLine($"Total matches: {TotalMatches}");

      foreach (KeyValuePair<string, int> match in Matches) {
        stringBuilder.AppendLine($"  '{match.Key}': {match.Value} times");
      }

      if (Errors.Count > 0) {
        stringBuilder.AppendLine($"Errors: {Errors.Count}");
      }

      return stringBuilder.ToString();
    }

  }

  // File index class
  public class FileIndex {

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

  // Simple console text editor
  public class TextEditor {

    private TextFile _currentFile;
    private TextFileHistory _history;
    private bool _isRunning;

    public void Run() {
      _isRunning = true;

      string welcomeMessage = "==========================================\n" +
                            "     TEXT FILE EDITOR\n" +
                            "==========================================";
      Console.WriteLine(welcomeMessage);

      while (_isRunning) {
        if (_currentFile == null) {
          ShowMainMenu();
        } else {
          ShowEditingMenu();
        }
      }
    }

    private void ShowMainMenu() {
      string menu = "\n--- MAIN MENU ---\n" +
                   "1. Open file\n" +
                   "2. Create new file\n" +
                   "3. Exit\n" +
                   "Your choice: ";
      Console.Write(menu);

      string choice = Console.ReadLine();

      switch (choice) {
        case "1":
          OpenFile();
          break;

        case "2":
          CreateNewFile();
          break;

        case "3":
          _isRunning = false;
          string goodbyeMessage = "\nGoodbye!";
          Console.WriteLine(goodbyeMessage);
          break;

        default:
          string invalidMessage = "Invalid choice. Please try again.";
          Console.WriteLine(invalidMessage);
          break;
      }
    }

    private void ShowEditingMenu() {
      string menu = "\n--- EDITING MENU ---\n" +
                   $"Current file: {_currentFile.FilePath}\n" +
                   "1. View content\n" +
                   "2. Edit content\n" +
                   "3. Save\n" +
                   "4. Save as...\n" +
                   "5. Undo\n" +
                   "6. Redo\n" +
                   "7. Reset to initial state\n" +
                   "8. JSON serialize\n" +
                   "9. XML serialize\n" +
                   "10. Close file\n" +
                   "Your choice: ";
      Console.Write(menu);

      string choice = Console.ReadLine();

      switch (choice) {
        case "1":
          ViewContent();
          break;

        case "2":
          EditContent();
          break;

        case "3":
          SaveFile();
          break;

        case "4":
          SaveAsFile();
          break;

        case "5":
          Undo();
          break;

        case "6":
          Redo();
          break;

        case "7":
          ResetToInitialState();
          break;

        case "8":
          JsonSerialize();
          break;

        case "9":
          XmlSerialize();
          break;

        case "10":
          CloseFile();
          break;

        default:
          string invalidMessage = "Invalid choice. Please try again.";
          Console.WriteLine(invalidMessage);
          break;
      }
    }

    private void OpenFile() {
      Console.Write("\nEnter file path to open: ");
      string filePath = Console.ReadLine();

      try {
        _currentFile = new TextFile(filePath);
        _history = new TextFileHistory(_currentFile);

        string successMessage = $"File loaded successfully: {filePath}";
        Console.WriteLine(successMessage);
      } catch (Exception exception) {
        string errorMessage = $"Error opening file: {exception.Message}";
        Console.WriteLine(errorMessage);
        _currentFile = null;
      }
    }

    private void CreateNewFile() {
      Console.Write("\nEnter file path for new file: ");
      string filePath = Console.ReadLine();

      Console.WriteLine("Enter content (type 'END' on a new line to finish):");

      StringBuilder contentBuilder = new StringBuilder();
      string line;

      while ((line = Console.ReadLine()) != "END") {
        contentBuilder.AppendLine(line);
      }

      try {
        _currentFile = new TextFile(filePath, contentBuilder.ToString());
        _history = new TextFileHistory(_currentFile);
        _currentFile.SaveToFile();

        string successMessage = $"File created successfully: {filePath}";
        Console.WriteLine(successMessage);
      } catch (Exception exception) {
        string errorMessage = $"Error creating file: {exception.Message}";
        Console.WriteLine(errorMessage);
        _currentFile = null;
      }
    }

    private void ViewContent() {
      string header = $"\n--- CONTENT OF {_currentFile.FilePath} ---\n";
      Console.WriteLine(header);
      Console.WriteLine(_currentFile.Content);
      Console.WriteLine("\n--- END OF CONTENT ---");
    }

    private void EditContent() {
      Console.WriteLine("\n--- EDITING MODE ---");
      Console.WriteLine("Enter new content (type 'SAVE' on a new line to save and exit):");

      StringBuilder contentBuilder = new StringBuilder();
      string line;

      while ((line = Console.ReadLine()) != "SAVE") {
        contentBuilder.AppendLine(line);
      }

      _history.SaveState();
      _currentFile.Content = contentBuilder.ToString();
      _currentFile.UpdateWordFrequency();

      string successMessage = "Content updated.";
      Console.WriteLine(successMessage);
    }

    private void SaveFile() {
      try {
        _currentFile.SaveToFile();
        string successMessage = "File saved successfully.";
        Console.WriteLine(successMessage);
      } catch (Exception exception) {
        string errorMessage = $"Error saving file: {exception.Message}";
        Console.WriteLine(errorMessage);
      }
    }

    private void SaveAsFile() {
      Console.Write("\nEnter new file path: ");
      string newPath = Console.ReadLine();

      try {
        string oldPath = _currentFile.FilePath;
        _currentFile.FilePath = newPath;
        _currentFile.SaveToFile();

        string successMessage = $"File saved as: {newPath}";
        Console.WriteLine(successMessage);
      } catch (Exception exception) {
        string errorMessage = $"Error saving file: {exception.Message}";
        Console.WriteLine(errorMessage);
      }
    }

    private void Undo() {
      try {
        _history.Undo();
        string successMessage = "Undo performed.";
        Console.WriteLine(successMessage);
      } catch (Exception exception) {
        string errorMessage = $"Cannot undo: {exception.Message}";
        Console.WriteLine(errorMessage);
      }
    }

    private void Redo() {
      try {
        _history.Redo();
        string successMessage = "Redo performed.";
        Console.WriteLine(successMessage);
      } catch (Exception exception) {
        string errorMessage = $"Cannot redo: {exception.Message}";
        Console.WriteLine(errorMessage);
      }
    }

    private void ResetToInitialState() {
      try {
        _history.ResetToInitialState();
        string successMessage = "Reset to initial state.";
        Console.WriteLine(successMessage);
      } catch (Exception exception) {
        string errorMessage = $"Cannot reset: {exception.Message}";
        Console.WriteLine(errorMessage);
      }
    }

    private void JsonSerialize() {
      try {
        Console.Write("\nEnter target file path (or press Enter for default): ");
        string targetPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(targetPath)) {
          _currentFile.JsonSerialize();
          string defaultMessage = $"JSON serialized to: {_currentFile.FilePath}.json";
          Console.WriteLine(defaultMessage);
        } else {
          _currentFile.JsonSerialize(targetPath);
          string customMessage = $"JSON serialized to: {targetPath}";
          Console.WriteLine(customMessage);
        }
      } catch (Exception exception) {
        string errorMessage = $"Error during JSON serialization: {exception.Message}";
        Console.WriteLine(errorMessage);
      }
    }

    private void XmlSerialize() {
      try {
        Console.Write("\nEnter target file path (or press Enter for default): ");
        string targetPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(targetPath)) {
          _currentFile.XmlSerialize();
          string defaultMessage = $"XML serialized to: {_currentFile.FilePath}.xml";
          Console.WriteLine(defaultMessage);
        } else {
          _currentFile.XmlSerialize(targetPath);
          string customMessage = $"XML serialized to: {targetPath}";
          Console.WriteLine(customMessage);
        }
      } catch (Exception exception) {
        string errorMessage = $"Error during XML serialization: {exception.Message}";
        Console.WriteLine(errorMessage);
      }
    }

    private void CloseFile() {
      _currentFile = null;
      _history = null;
      string closeMessage = "File closed.";
      Console.WriteLine(closeMessage);
    }

  }

  // Indexing application
  public class IndexingApplication {

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

    private FileIndex _currentIndex;

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

        // Save index
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

  // Main program
  class Program {

    static void Main(string[] args) {
      string headerMessage = "==========================================\n" +
                           "     TEXT FILE MANAGEMENT SYSTEM\n" +
                           "==========================================";
      Console.WriteLine(headerMessage);

      bool isRunning = true;

      while (isRunning) {
        string menu = "\n--- MAIN APPLICATION MENU ---\n" +
                     "1. Text Editor\n" +
                     "2. File Indexing Application\n" +
                     "3. Exit\n" +
                     "Your choice: ";
        Console.Write(menu);

        string choice = Console.ReadLine();

        switch (choice) {
          case "1":
            TextEditor editor = new TextEditor();
            editor.Run();
            break;

          case "2":
            IndexingApplication indexingApp = new IndexingApplication();
            indexingApp.Run();
            break;

          case "3":
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
  }
}