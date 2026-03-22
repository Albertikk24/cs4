using System.Collections.Generic;

namespace TextFileEditor {

  public interface ITextFileSearcher {

    void AddFilePattern(string pattern);
    List<string> GetFilePatterns();
    List<SearchResult> SearchByKeywords(List<string> keywords, bool caseSensitive = false);
    FileIndex CreateIndex();

  }
}