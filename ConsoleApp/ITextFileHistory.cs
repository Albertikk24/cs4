namespace TextFileEditor {

  public interface ITextFileHistory {

    bool CanUndo { get; }
    bool CanRedo { get; }

    void SaveState();
    void Undo();
    void Redo();
    void ResetToInitialState();

  }
}