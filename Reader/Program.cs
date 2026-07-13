using ReaderWriterApp.Shared;

namespace Reader;

class Program
{
    static long lastPosition = 0;
    static DateTime lastEventTime = DateTime.MinValue;
    static readonly TimeSpan DebounceInterval = TimeSpan.FromMilliseconds(300);

    static void Main(string[] args)
    {
        Console.WriteLine("You are the Reader.");
        Console.WriteLine($"Watching file: {Protocol.SharedFilePath}");
        Console.WriteLine();

        if (!File.Exists(Protocol.SharedFilePath))
        {
            File.Create(Protocol.SharedFilePath).Dispose();
        }

        lastPosition = new FileInfo(Protocol.SharedFilePath).Length;

        using var watcher = new FileSystemWatcher(
            Path.GetDirectoryName(Path.GetFullPath(Protocol.SharedFilePath))!,
            Path.GetFileName(Protocol.SharedFilePath));

        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
        watcher.Changed += OnFileChanged;
        watcher.EnableRaisingEvents = true;

        Console.WriteLine("Waiting for messages... (Ctrl+C to exit)");
        Console.WriteLine();

        while (true)
        {
            Thread.Sleep(500);
        }
    }

    static void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        var now = DateTime.Now;
        if (now - lastEventTime < DebounceInterval)
            return;
        lastEventTime = now;

        Thread.Sleep(100);

        try
        {
            ReadNewContent();
        }
        catch (IOException)
        {
            Thread.Sleep(100);
            try { ReadNewContent(); } catch { }
        }
    }

    static void ReadNewContent()
    {
        using var stream = new FileStream(
            Protocol.SharedFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite);

        stream.Seek(lastPosition, SeekOrigin.Begin);

        using var reader = new StreamReader(stream);
        string newContent = reader.ReadToEnd();
        lastPosition = stream.Length;

        if (string.IsNullOrWhiteSpace(newContent))
            return;

        var messages = newContent.Split(
            Protocol.MessageDelimiter,
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var message in messages)
        {
            var trimmed = message.Trim('\r', '\n');
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            DisplayMessage(trimmed);
        }
    }

    static void DisplayMessage(string text)
    {
        bool isParagraph = text.Contains('\n');

        if (isParagraph)
        {
            Console.WriteLine("Writer wrote:");
            Console.WriteLine("----------------");
            Console.WriteLine(text);
            Console.WriteLine("----------------");
        }
        else
        {
            Console.WriteLine($"Writer wrote: {text}");
        }

        Console.WriteLine();
    }
}