using System;

namespace TextFileEditor {

  public class TextFileMemento : ITextFileMemento {

    public string Content { get; private set; }
    public DateTime Timestamp { get; private set; }

    public TextFileMemento(string content) {
      Content = content;
      Timestamp = DateTime.Now;
    }
  }
}