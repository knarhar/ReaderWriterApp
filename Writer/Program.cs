using ReaderWriterApp.Shared;
using System.Text;

namespace Writer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args[0] == "--path")
            {
                Protocol.SharedFilePath = args[1];
            }
            else
            {
                Console.WriteLine("Runtime Arguments Error");
                return;
            }

            Console.WriteLine("========== Writer ==========");
            Console.WriteLine("Modes: line (default) | paragraph");
            Console.WriteLine("Commands: /mode line | /mode paragraph | /send | /clear | /exit");
            Console.WriteLine();

            string mode = "line";
            var buffer = new StringBuilder();

            using var sourceFile = new FileStream(
                Protocol.SharedFilePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read
            );
            using var writer = new StreamWriter(sourceFile);

            while (true)
            {
                Console.Write(mode == "line" ? "> " : ".. ");
                string? input = Console.ReadLine();

                if (input is null) continue;

                switch (input.Trim().ToLower())
                {
                    case "/exit":
                        return;

                    case "/mode line":
                        mode = "line";
                        Console.WriteLine("[Switched to line mode]");
                        continue;

                    case "/mode paragraph":
                        mode = "paragraph";
                        Console.WriteLine("[Switched to paragraph mode. Type /send to submit, /clear to discard.]");
                        continue;

                    case "/clear":
                        buffer.Clear();
                        Console.WriteLine("[Buffer cleared]");
                        continue;

                    case "/send":
                        if (mode == "paragraph")
                        {
                            SendMessage(writer, buffer.ToString());
                            buffer.Clear();
                        }
                        continue;
                }

                if (mode == "line")
                {
                    SendMessage(writer, input);
                }
                else
                {
                    buffer.AppendLine(input);
                }
            }
        }

        static void SendMessage(StreamWriter writer, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            writer.WriteLine(text.TrimEnd());
            writer.WriteLine(Protocol.MessageDelimiter);
            writer.Flush();

            Console.WriteLine("[Sent]");
        }
    }
}