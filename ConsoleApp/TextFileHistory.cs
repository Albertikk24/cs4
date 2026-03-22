using System;
using System.Collections.Generic;

namespace TextFileEditor {

  public class TextFileHistory : ITextFileHistory {

    private List<TextFileMemento> _undoStack = new List<TextFileMemento>();
    private List<TextFileMemento> _redoStack = new List<TextFileMemento>();
    private TextFileOriginator _originator;
    private int _maxHistorySize;
    private TextFileMemento _initialState;

    public bool CanUndo => _undoStack.Count > 1;
    public bool CanRedo => _redoStack.Count > 0;

    public TextFileHistory(TextFile textFile, int maxHistorySize = 50) {
      _originator = new TextFileOriginator(textFile);
      _maxHistorySize = maxHistorySize;

      _initialState = _originator.SaveState();
      _undoStack.Add(_initialState);
    }

    public void SaveState() {
      TextFileMemento memento = _originator.SaveState();
      _undoStack.Add(memento);

      if (_undoStack.Count > _maxHistorySize) {
        _undoStack.RemoveAt(0);
      }

      _redoStack.Clear();
    }

    public void Undo() {
      if (!CanUndo) {
        if (_undoStack.Count == 1) {
          _originator.RestoreState(_initialState);
        }
        return;
      }

      TextFileMemento currentState = _originator.SaveState();
      _redoStack.Add(currentState);

      _undoStack.RemoveAt(_undoStack.Count - 1);

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
}