using Microsoft.KernelMemory;

namespace ChuckPilot.Api
{
    internal static class MemoryManager
    {
        internal static IKernelMemory _memory;

        internal static async Task InitializeMemory()
        {
            _memory = new MemoryWebClient("http://127.0.0.1:5000"); // <== URL where the web service is running

            var docsFilesPath = LocalFiles.GetPath("Docs");

            // Import a file specifying a Document ID, User and Tags
            await _memory.ImportDocumentAsync(new Microsoft.KernelMemory.Document("ChuckDocs")
                .AddFile($"{docsFilesPath}\\Chuck Norris - Personal Life.docx")
                .AddFile($"{docsFilesPath}\\Chuck Norris - Professional Life.docx")
                .AddFile($"{docsFilesPath}\\Discography.txt"));
        }

        internal static async Task<string> AskToMemoryAsync(string ask)
        {
            var answer = await _memory.AskAsync(ask);
            return answer.Result;
        }

    }
}
