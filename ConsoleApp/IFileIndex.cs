using System;
using System.Collections.Generic;

namespace TextFileEditor {

  public interface IFileIndex {

    string RootDirectory { get; }
    DateTime IndexedAt { get; }
    List<TextFile> Files { get; }
    Dictionary<string, List<string>> KeywordIndex { get; }

    void AddFile(TextFile textFile);
    List<string> FindFilesByKeyword(string keyword);
    Dictionary<string, int> GetKeywordStatistics();

  }
}