namespace TextFileEditor {

  public interface ITextFileOriginator {

    TextFileMemento SaveState();
    void RestoreState(TextFileMemento memento);

  }
}