using System.Collections.Generic;

namespace TextFileEditor {

  public interface ISearchResult {

    string FilePath { get; }
    Dictionary<string, int> Matches { get; }
    List<string> Errors { get; }
    int TotalMatches { get; }

    void AddMatch(string keyword, int count);
    void AddError(string error);

  }
}