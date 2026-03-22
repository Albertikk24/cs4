using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TextFileEditor {

  [Serializable]
  public class TextFile : ITextFileSerialization {

    public string FilePath { get; set; }
    public string Content { get; set; }
    public DateTime LastModified { get; set; }
    public Dictionary<string, int> WordFrequency { get; set; }

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

    public void LoadFromFile() {
      if (!File.Exists(FilePath)) {
        throw new FileNotFoundException(FilePath);
      }

      Content = File.ReadAllText(FilePath, Encoding.UTF8);
      LastModified = File.GetLastWriteTime(FilePath);
      UpdateWordFrequency();
    }

    public void SaveToFile() {
      string directory = Path.GetDirectoryName(FilePath);
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
        Directory.CreateDirectory(directory);
      }

      File.WriteAllText(FilePath, Content, Encoding.UTF8);
      LastModified = DateTime.Now;
    }

    public void UpdateWordFrequency() {
      WordFrequency.Clear();

      if (string.IsNullOrWhiteSpace(Content)) {
        return;
      }

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

    public void BinarySerialize(string targetFilePath = null) {
      string filePath = targetFilePath ?? FilePath + ".bin";

      using (FileStream fileStream = new FileStream(filePath, FileMode.Create)) {
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(fileStream, this);
      }
    }

    public static TextFile BinaryDeserialize(string filePath) {
      if (!File.Exists(filePath)) {
        throw new FileNotFoundException(filePath);
      }

      using (FileStream fileStream = new FileStream(filePath, FileMode.Open)) {
        BinaryFormatter formatter = new BinaryFormatter();
        return (TextFile)formatter.Deserialize(fileStream);
      }
    }

    public void XmlSerialize(string targetFilePath = null) {
      string filePath = targetFilePath ?? FilePath + ".xml";

      XmlSerializer serializer = new XmlSerializer(typeof(TextFile));
      using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8)) {
        serializer.Serialize(writer, this);
      }
    }

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
}