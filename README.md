# Reader/Writer Console App

Two console apps that communicate through a shared text file — a **Writer** that sends messages, and a **Reader** that watches the file and displays new messages in real time.

## Project Structure

```
ReaderWriterApp/
├── Shared/     # Common constants (file path, message delimiter)
├── Writer/     # Sends messages to the shared file
└── Reader/     # Watches the file and displays new messages
```

## How It Works

- Writer appends messages to a shared file, each followed by a delimiter (`---END---`).
- Reader uses `FileSystemWatcher` to detect changes and reads only the new content since its last read position.
- Delimiter allows multi-line (paragraph) messages to be read as a single block.

## Writer

Two typing modes:
- **Line mode** (default) — Enter sends each line instantly.
- **Paragraph mode** — type multiple lines, then `/send` to submit as one message.

Commands: `/mode line`, `/mode paragraph`, `/send`, `/clear`, `/exit`

## Reader

Watches the shared file and prints new messages as they arrive:
```
Writer wrote: hello
```
Paragraph messages are printed as a block between separators.
```
Writer wrote:
----------------
<paragraph text>
<paragraph text>
<paragraph text>
----------------
```


## Running

Run both from the same folder (or point both at the same file with `--path`):

```bash
cd Writer && dotnet run --path shared.txt
```
```bash
cd Reader && dotnet run --path shared.txt
```

## Notes

- Writer opens/writes/closes the file per message so changes are picked up immediately.
- Reader debounces file-change events (multiple events can fire per write).
