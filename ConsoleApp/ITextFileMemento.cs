using System;

namespace TextFileEditor {

  public interface ITextFileMemento {

    string Content { get; }
    DateTime Timestamp { get; }

  }
}