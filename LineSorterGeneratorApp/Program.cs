using System.CommandLine;
using System.Diagnostics;
using System.Text;

class Program {
    public static async Task<int> Main(string[] args) {
        var outputFileOption = new Option<FileInfo>(
            name: "--output",
            () => new FileInfo("output.txt"),
            description: "Path pointing where to put the results. Will be overwritten"
        );

        var targetFileSizeOption = new Option<long>(
            name: "--target-size",
            () => 1073741824, // 1GB,
            description: "Target file size in bytes. Note: output can be a bit bigger just to avoid cutting the line content"
        );

        var binFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var dictionaryOption = new Option<FileInfo>(
            name: "--dictionary",
            () => new FileInfo(Path.Combine(binFolder ?? "", "words.txt")),
            description: "Dictionary of words separated with new lines that will be used to generate the result file. If not specified the bundled one will be used"
        );

        targetFileSizeOption.AddValidator(result => {
            if (result.GetValueForOption(targetFileSizeOption) < 1)
            {
                result.ErrorMessage = "Must be greater than 0";
            }
        });

        var rootCommand = new RootCommand("Application generating [num]. [text] line filled text files");
        rootCommand.AddOption(outputFileOption);
        rootCommand.AddOption(targetFileSizeOption);
        rootCommand.AddOption(dictionaryOption);

        rootCommand.SetHandler(Run, outputFileOption, dictionaryOption, targetFileSizeOption);

        return await rootCommand.InvokeAsync(args);
    }

    static void Run(FileInfo outputFile, FileInfo dictionaryFile, long bytesToWrite) {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var words = File.ReadAllLines(dictionaryFile.FullName);
        var random = new Random();
        using var outputStream = File.Open(outputFile.FullName, FileMode.Create, FileAccess.Write);

        while (bytesToWrite > 0) {
            var lineTextByteSize = random.Next(1, 2000);
            var lineNumber = random.Next();
            var stringBuilder = new StringBuilder();

            while (lineTextByteSize > 0) {
                var word = words[random.Next(words.Length)];

                // if it's the first word then we uppercase it
                if (stringBuilder.Length == 0) {
                    word = word[0].ToString().ToUpper() + word[1..];
                }

                stringBuilder.Append($" {word}");
                lineTextByteSize -= Encoding.ASCII.GetByteCount(word) + 1;
            }

            var line = $"{lineNumber}.{stringBuilder}{Environment.NewLine}";

            // using ascii as its much simpler
            // and some linux readers have problems with unicode
            var lineByteSize = Encoding.ASCII.GetByteCount(line);
            outputStream.Write(Encoding.ASCII.GetBytes(line), 0, lineByteSize);

            bytesToWrite -= lineByteSize;
        }

        outputStream.Close();
        
        outputFile.Refresh();

        stopWatch.Stop();
        // file size will be bigger since
        // we dont want to have lines that are cut in the middle
        Console.WriteLine($"{outputFile.Length} bytes written to {outputFile.FullName} in {stopWatch.Elapsed}");
    }
}