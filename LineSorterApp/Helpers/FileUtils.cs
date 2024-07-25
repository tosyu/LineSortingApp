using System.Text;
using LineSorterApp.DataStructures;

namespace LineSorterApp.Helpers;

public static class FileUtils
{
    public static List<FileInfo> SortAndSplitBySize(this FileInfo inputFile, long sizeLimit, FileInfo temporaryFolder)
    {
        List<FileInfo> temporaryFiles = [];

        using var inputReader = new StreamReader(inputFile.FullName, Encoding.ASCII);
        var tempFileCount = Math.Ceiling((double)inputFile.Length / sizeLimit);
        string? line = null;
        var comparer = new RowComparer();

        // split main file in to smaller than available memory chunks
        // to avoid out of memory exception
        for (var i = 0; i < tempFileCount; i++)
        {
            var tempFile = new FileInfo(Path.Join(Path.GetDirectoryName(temporaryFolder.FullName), Path.GetFileName(Path.GetTempFileName())));
            List<Row> rows = [];
            long tempFileSize = 0;
            while (inputReader.Peek() >= 0)
            {
                // advance seek only if start or line written
                line ??= inputReader.ReadLine();

                if (line == null)
                {
                    break;
                }

                var lineSize = Encoding.ASCII.GetByteCount(line);

                if (tempFileSize + lineSize > sizeLimit)
                {
                    break;
                }

                rows.Add(line.ToRow());
                tempFileSize += lineSize;
                line = null;
            }

            rows.Sort(comparer);
            rows.WriteAllRows(tempFile.FullName);
            temporaryFiles.Add(tempFile);
        }

        inputReader.Close();

        return temporaryFiles;
    }

    public static void MergeSplitsInto(this List<FileInfo> inputFiles, FileInfo outputFile)
    {
        using var outputFileStream = new StreamWriter(outputFile.FullName, false, Encoding.ASCII);
        var queue = new MinQueue();
        List<StreamReader> streams = [];

        // setup queue
        foreach (var file in inputFiles)
        {
            var reader = new StreamReader(file.FullName, Encoding.ASCII);
            streams.Add(reader);
            if (reader.Peek() >= 0)
            {
                var line = reader.ReadLine();
                if (line != null)
                {
                    queue.Queue(line, reader);
                }
            }
        }

        do
        {
            var node = queue.Dequeue();
            string? line;
            if (node != null)
            {
                outputFileStream.WriteLine(node.data.ToFormattedString());
                if (node.stream.Peek() >= 0 && (line = node.stream.ReadLine()) != null)
                {
                    queue.Queue(line, node.stream);
                }
            }
        } while (queue.IsEmpty == false);

        // cleanup
        streams.ForEach(stream =>
        {
            if (stream.BaseStream != null)
            {
                stream.Close();
            }
        });
    }
}