namespace TextFileEditor {

  public interface ITextFileSerialization {

    void BinarySerialize(string targetFilePath = null);
    void XmlSerialize(string targetFilePath = null);

  }
}