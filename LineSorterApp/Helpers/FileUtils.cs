using System.Text;
using LineSorterApp.DataStructrues;

namespace LineSorterApp.Helpers;

public static class FileUtils
{
    public static List<FileInfo> SplitBySize(this FileInfo inputFile, long sizeLimit, FileInfo temporaryFolder)
    {
        List<FileInfo> temporaryFiles = [];

        using var inputReader = new StreamReader(inputFile.FullName, Encoding.ASCII);
        var tempFileCount = Math.Ceiling((double)inputFile.Length / sizeLimit);
        string? line = null;

        // split main file in to smaller than available memory chunks
        // to avoid out of memory exception        
        for (var i = 0; i < tempFileCount; i++)
        {
            var tempFile = new FileInfo(Path.Join(Path.GetDirectoryName(temporaryFolder.FullName), Path.GetFileName(Path.GetTempFileName())));
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
        using var outputFileStream = new StreamWriter(outputFile.FullName, false, Encoding.ASCII);
        var queue = new MinQueue();
        
        // setup queue
        foreach (var file in inputFiles) {
            var reader = new StreamReader(file.FullName, Encoding.ASCII);
            if (reader.Peek() >= 0) {
                var line = reader.ReadLine();
                if (line != null) {
                    queue.Qeueue(line, reader);
                }
            }
        }

        do {
            var node = queue.Dequeue();
            string? line;
            if (node != null) {
                outputFileStream.WriteLine(node.data);
                if (node.stream.Peek() >= 0 && (line = node.stream.ReadLine()) != null) {
                    queue.Qeueue(line, node.stream);
                } else {
                    node.stream.Close();
                }
            }
        } while (queue.IsEmpty == false);
    }
}