using System.Text;

namespace LineSorterApp.Helpers;

public static class FileUtils
{
    public static List<FileInfo> SplitBySize(this FileInfo inputFile, long sizeLimit)
    {
        List<FileInfo> temporaryFiles = [];

        using var inputReader = new StreamReader(inputFile.FullName, Encoding.ASCII);
        var tempFileCount = Math.Ceiling((double)inputFile.Length / sizeLimit);
        string? line = null;

        // split main file in to smaller than available memory chunks
        // to avoid out of memory exception        
        for (var i = 0; i < tempFileCount; i++)
        {
            var tempFile = new FileInfo(Path.GetTempFileName());
            var tempWrite = new StreamWriter(tempFile.FullName, false, Encoding.ASCII);
            long tempFileSize = 0;
            while (inputReader.Peek() >= 0)
            {
                // advance seek only if start or line written
                line ??= inputReader.ReadLine();
                if (line != null)
                {
                    var lineSize = Encoding.ASCII.GetByteCount(line);
                    if (tempFileSize + lineSize < sizeLimit)
                    {
                        tempWrite.WriteLine(line);
                        tempFileSize += lineSize;
                        line = null;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            tempWrite.Close();
            temporaryFiles.Add(tempFile);
        }

        inputReader.Close();

        return temporaryFiles;
    }

    public static void SortSplits(this List<FileInfo> inputFiles)
    {
        var comparer = new LineComparer();
        foreach (var file in inputFiles) {
            var lines = File.ReadAllLines(file.FullName).ToList();

            // sorting purely in memory
            lines.Sort(comparer);

            File.WriteAllLines(file.FullName, lines);
        }
    }

    public static void MergeSplitsInto(this List<FileInfo> inputFiles, FileInfo outputFile)
    {
        using var outputFileStream = File.Open(outputFile.FullName, FileMode.Create, FileAccess.Write);
        using var firstFileStream = File.Open(inputFiles.First().FullName, FileMode.Open, FileAccess.Read);

        firstFileStream.CopyTo(outputFileStream);

        outputFileStream.Close();
        firstFileStream.Close();

        inputFiles.Skip(1).ToList().ForEach(file => MergeFilesPair(outputFile, file));
    }

    private static void MergeFilesPair(FileInfo firstFile, FileInfo secondFile) {
        var comparer = new LineComparer();
        var temporaryFile = new FileInfo(Path.GetTempFileName());
        using var temporaryFileStream = new StreamWriter(temporaryFile.FullName, false, Encoding.ASCII);
        using var firstFileReader = new StreamReader(firstFile.FullName, Encoding.ASCII);
        using var secondFileReader = new StreamReader(secondFile.FullName, Encoding.ASCII);

        string? firstLine = null;
        string? secondLine = null;

        // merge files keeping the sorting order
        while (firstFileReader.Peek() >= 0 && secondFileReader.Peek() >= 0) {
            firstLine ??= firstFileReader.ReadLine();
            secondLine ??= secondFileReader.ReadLine();

            if (firstLine != null && secondLine != null) {
                var compareLines = comparer.Compare(firstLine, secondLine);
                if (compareLines > 1) {                    
                    temporaryFileStream.WriteLine(firstLine);
                    firstLine = null;
                } else {
                    temporaryFileStream.WriteLine(secondLine);
                    secondLine = null;                    
                }
            } else {               
                break;
            }          
        }

        // save anything that was left from the loop (line and file content)
        if (firstLine != null) {
            temporaryFileStream.WriteLine(firstLine);
        }

        if (secondLine != null) {
            temporaryFileStream.WriteLine(secondLine);
        }

        while (firstFileReader.Peek() >= 0) {
            var line = firstFileReader.ReadLine();
            if (line != null) {
                temporaryFileStream.WriteLine(line);
            }
        }

        while (secondFileReader.Peek() >= 0) {
            var line = secondFileReader.ReadLine();
            if (line != null) {
                temporaryFileStream.WriteLine(line);
            }
        }

        firstFileReader.Close();
        secondFileReader.Close();
        temporaryFileStream.Close();

        // overwrite the base file with merged file data
        using var sourceStream = File.Open(temporaryFile.FullName, FileMode.Open, FileAccess.Read);
        using var destinationStream = File.Open(firstFile.FullName, FileMode.Open, FileAccess.Write);

        sourceStream.CopyTo(destinationStream);

        destinationStream.Close();
        temporaryFileStream.Close();

        // cleanup
        File.Delete(temporaryFile.FullName);
    }
}