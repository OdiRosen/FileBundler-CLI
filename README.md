# FileBundler-CLI
This project is a powerful Command Line Interface (CLI) tool designed to streamline the code
submission process for developers and students. It automates the task of bundling multiple source
code files into a single, well-organized document while maintaining structural integrity and clean formatting.

Core Features
- Smart Code Bundling: Automatically aggregates source files from complex directory structures
into a single output file based on specific programming languages or an "all" flag.
- Advanced Filtering: Implemented a robust exclusion logic to automatically ignore build artifacts
  and temporary folders (e.g., bin, obj, debug), ensuring only relevant source code is included.
- Dynamic Content Formatting: - Code Cleanup: An optional feature to remove empty lines,
  reducing file size and improving readability.
  - Source Attribution: Includes a "Note" feature that adds the relative path of each original file
    as a header within the          bundle.
- Response File (RSP) Support: A custom-built interactive command (create-rsp) that guides
  the user through the configuration process and generates a persistent response file, allowing for
  complex executions without re-typing arguments.
- Flexible Sorting: Supports multiple organization strategies, allowing files to be sorted
  alphabetically by name or grouped by file extension.

Tech Stack
- Framework: .NET 8 / .NET Console Application.
- Primary Library: System.CommandLine (Beta) – utilized for sophisticated argument parsing,
  alias support, and automated help generation.
- Language: C#.
- IO Operations: Advanced System.IO usage for recursive directory traversal and relative path calculation.

How it Works
1. Input Parsing: The tool uses a RootCommand to distinguish between the bundle action and the create-rsp helper.
2. File Discovery: It scans the current directory recursively, applying a split-path validation to strictly exclude forbidden directories like bin or obj.
3. Transformation Pipeline: - Files are filtered by a dictionary-mapped extension list.
   - If enabled, a line-by-line processor strips whitespace.
   - Global sorting is applied to the final list before writing to the stream.
4. Interactive Configuration: The create-rsp command acts as a "wizard," collecting user input through the console and formatting it into a standard CLI-compatible .rsp file.

Setup & Usage
To use this tool:
1. Clone the repository.
2. Build the project:
   dotnet build
3. Run the bundle command:
   dotnet run -- bundle --language cs --output myBundle.txt --note --sort name --author "Your Name"
4. Or use the Interactive Wizard:
   dotnet run -- create-rsp
# This will generate an 'options.rsp' file.
# Then run:
dotnet run -- @options.rsp
