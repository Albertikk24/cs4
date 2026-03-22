using System;
using System.Text;

namespace TextFileEditor {

  public class TextEditor : ITextEditor {

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
                   "8. Binary serialize\n" +
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
          BinarySerialize();
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

    private void BinarySerialize() {
      try {
        Console.Write("\nEnter target file path (or press Enter for default): ");
        string targetPath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(targetPath)) {
          _currentFile.BinarySerialize();
          string defaultMessage = $"Binary serialized to: {_currentFile.FilePath}.bin";
          Console.WriteLine(defaultMessage);
        } else {
          _currentFile.BinarySerialize(targetPath);
          string customMessage = $"Binary serialized to: {targetPath}";
          Console.WriteLine(customMessage);
        }
      } catch (Exception exception) {
        string errorMessage = $"Error during binary serialization: {exception.Message}";
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
}