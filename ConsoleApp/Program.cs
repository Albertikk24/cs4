using System;

namespace TextFileEditor {

  class Program {

    static void Main(string[] args) {
      string headerMessage = "==========================================\n" +
                           "     TEXT FILE MANAGEMENT SYSTEM\n" +
                           "==========================================";
      Console.WriteLine(headerMessage);

      bool isRunning = true;

      while (isRunning) {
        string menu = "\n--- MAIN APPLICATION MENU ---\n" +
                     "1. Text Editor\n" +
                     "2. File Indexing Application\n" +
                     "3. Exit\n" +
                     "Your choice: ";
        Console.Write(menu);

        string choice = Console.ReadLine();

        switch (choice) {
          case "1":
            ITextEditor editor = new TextEditor();
            editor.Run();
            break;

          case "2":
            IIndexingApplication indexingApp = new IndexingApplication();
            indexingApp.Run();
            break;

          case "3":
            isRunning = false;
            string goodbyeMessage = "\nGoodbye!";
            Console.WriteLine(goodbyeMessage);
            break;

          default:
            string invalidMessage = "Invalid choice. Please try again.";
            Console.WriteLine(invalidMessage);
            break;
        }
      }
    }
  }
}