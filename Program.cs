using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // --- 1. הגדרת האופציות (Options) לפי דרישות הפרויקט ---

        var languageOption = new Option<string>(
            aliases: new[] { "--language", "-l" },
            description: "Required: Programming languages (e.g., 'cs', 'java', 'js') or 'all'")
        { IsRequired = true };

        var outputOption = new Option<FileInfo>(
            aliases: new[] { "--output", "-o" },
            description: "File path and name for the bundled file");

        var noteOption = new Option<bool>(
            aliases: new[] { "--note", "-n" },
            description: "Add a comment with the source file name and relative path");

        var sortOption = new Option<string>(
            aliases: new[] { "--sort", "-s" },
            description: "Sort files by 'name' (alphabetical) or 'type' (extension)",
            getDefaultValue: () => "name");

        var removeEmptyLinesOption = new Option<bool>(
            aliases: new[] { "--remove-empty-lines", "-r" },
            description: "Remove empty lines from the source code");

        var authorOption = new Option<string>(
            aliases: new[] { "--author", "-a" },
            description: "Add the name of the author at the top of the bundle");

        // --- 2. יצירת פקודת ה-bundle ---

        var bundleCommand = new Command("bundle", "Bundle code files into a single file");
        bundleCommand.AddOption(languageOption);
        bundleCommand.AddOption(outputOption);
        bundleCommand.AddOption(noteOption);
        bundleCommand.AddOption(sortOption);
        bundleCommand.AddOption(removeEmptyLinesOption);
        bundleCommand.AddOption(authorOption);

        bundleCommand.SetHandler((string lang, FileInfo output, bool note, string sort, bool removeEmpty, string author) =>
        {
            try
            {
                // בדיקת תקינות נתיב הפלט (Validation)
                if (output == null)
                {
                    Console.WriteLine("ERROR: Output file path is required.");
                    return;
                }

                // מפת שפות מורחבת (כולל שפות נוספות שביקשת)
                var extensionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "cs", ".cs" }, { "csharp", ".cs" },
                    { "java", ".java" },
                    { "py", ".py" }, { "python", ".py" },
                    { "js", ".js" }, { "javascript", ".js" },
                    { "ts", ".ts" }, { "typescript", ".ts" },
                    { "html", ".html" }, { "css", ".css" },
                    { "cpp", ".cpp" }, { "h", ".h" },
                    { "sql", ".sql" }, { "json", ".json" }
                };

                string currentDirectory = Directory.GetCurrentDirectory();

                // איסוף קבצים וסינון תיקיות bin/obj/debug (דרישת פרויקט)
                var allFiles = Directory.GetFiles(currentDirectory, "*.*", SearchOption.AllDirectories)
                    .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(part =>
                        part.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                        part.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
                        part.Equals("debug", StringComparison.OrdinalIgnoreCase)))
                    .Where(f => !f.EndsWith(output.Name)) // מניעת כניסת קובץ הפלט לתוך עצמו
                    .ToList();

                // סינון לפי שפות
                List<string> selectedExtensions = new List<string>();
                if (lang.ToLower() == "all")
                {
                    selectedExtensions = extensionMap.Values.Distinct().ToList();
                }
                else
                {
                    var requestedLangs = lang.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var l in requestedLangs)
                    {
                        if (extensionMap.TryGetValue(l.Trim(), out string ext))
                            selectedExtensions.Add(ext);
                    }
                }

                var filesToBundle = allFiles
                    .Where(f => selectedExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToList();

                // מיון קבצים (דרישת sort)
                filesToBundle = sort.ToLower() == "type"
                    ? filesToBundle.OrderBy(f => Path.GetExtension(f)).ThenBy(f => Path.GetFileName(f)).ToList()
                    : filesToBundle.OrderBy(f => Path.GetFileName(f)).ToList();

                if (!filesToBundle.Any())
                {
                    Console.WriteLine("No matching files found for the selected language(s).");
                    return;
                }

                // כתיבה לקובץ
                using (var writer = new StreamWriter(output.FullName))
                {
                    if (!string.IsNullOrEmpty(author))
                    {
                        writer.WriteLine($"// Author: {author}");
                        writer.WriteLine();
                    }

                    foreach (var file in filesToBundle)
                    {
                        if (note)
                        {
                            string relativePath = Path.GetRelativePath(currentDirectory, file);
                            writer.WriteLine($"// --- Source: {relativePath} ---");
                        }

                        var lines = File.ReadAllLines(file);
                        foreach (var line in lines)
                        {
                            if (removeEmpty && string.IsNullOrWhiteSpace(line)) continue;
                            writer.WriteLine(line);
                        }
                        writer.WriteLine(); // רווח בין קבצים
                    }
                }

                Console.WriteLine($"SUCCESS: {filesToBundle.Count} files bundled into {output.FullName}");
            }
            catch (DirectoryNotFoundException) { Console.WriteLine("ERROR: Invalid directory path."); }
            catch (UnauthorizedAccessException) { Console.WriteLine("ERROR: No permission to write to this location."); }
            catch (Exception ex) { Console.WriteLine($"ERROR: {ex.Message}"); }

        }, languageOption, outputOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

        // --- 3. יצירת פקודת create-rsp (Response File) ---

        var createRspCommand = new Command("create-rsp", "Interactive helper to create a response file");

        createRspCommand.SetHandler(() =>
        {
            Console.WriteLine("=== Response File Creator ===");

            Console.Write("Enter languages (e.g., 'cs, java' or 'all'): ");
            var lang = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(lang)) { Console.Write("Required! Languages: "); lang = Console.ReadLine(); }

            Console.Write("Enter output file name/path: ");
            var output = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(output)) { Console.Write("Required! Output: "); output = Console.ReadLine(); }

            Console.Write("Add source notes? (y/n): ");
            bool note = Console.ReadLine()?.ToLower() == "y";

            Console.Write("Sort by [name/type]: ");
            var sort = Console.ReadLine();
            if (string.IsNullOrEmpty(sort)) sort = "name";

            Console.Write("Remove empty lines? (y/n): ");
            bool removeEmpty = Console.ReadLine()?.ToLower() == "y";

            Console.Write("Author name (optional): ");
            var author = Console.ReadLine();

            // בניית תוכן קובץ ה-RSP (במבנה של טוקנים נפרדים לקריאות)
            var rspContent = new List<string>();
            rspContent.Add($"bundle");
            rspContent.Add($"--language {lang}");
            rspContent.Add($"--output \"{output}\"");
            if (note) rspContent.Add("--note");
            if (removeEmpty) rspContent.Add("--remove-empty-lines");
            rspContent.Add($"--sort {sort}");
            if (!string.IsNullOrEmpty(author)) rspContent.Add($"--author \"{author}\"");

            File.WriteAllLines("options.rsp", rspContent);
            Console.WriteLine("\nSUCCESS! 'options.rsp' created.");
            Console.WriteLine("To run: dotnet run -- @options.rsp");
        });

        // --- 4. הגדרת Root וביצוע ---
        var rootCommand = new RootCommand("File Bundler CLI Tool");
        rootCommand.AddCommand(bundleCommand);
        rootCommand.AddCommand(createRspCommand);

        return await rootCommand.InvokeAsync(args);
    }
}