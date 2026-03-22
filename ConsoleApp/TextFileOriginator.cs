using System;

namespace TextFileEditor {

  public class TextFileOriginator : ITextFileOriginator {

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
}