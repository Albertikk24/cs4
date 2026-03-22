namespace TextFileEditor {

  public class FileNotFoundException : TextFileException {

    public FileNotFoundException(string filePath) : base($"File not found: {filePath}") { }

  }
}