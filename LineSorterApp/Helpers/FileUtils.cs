using System.Diagnostics;
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
            var tempFile = new FileInfo(Path.Join(temporaryFolder.FullName, Path.GetFileName(Path.GetTempFileName())));
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

    public static void MergeSplitsInto(this List<FileInfo> inputFiles, FileInfo outputFile, long bufferSize)
    {
        var bufferSizePerFile = Math.Max(bufferSize / (inputFiles.Count + 1), 134217728); // 128M min
        using var outputFileStream = new StreamWriter(outputFile.FullName, false, Encoding.ASCII);
        var sortedQueue = new SortedQueue();
        var readBuffers = new Dictionary<StreamReader, Queue<string>>();
        var streamsToDelete = new List<StreamReader>();
        var sw = new Stopwatch();

        // setup buffers
        inputFiles.ForEach(file => readBuffers.Add(new StreamReader(file.FullName, Encoding.ASCII, false), []));

        while (true)
        {
            // loop that will either fill the read queue or enqueue the sorted queue or close the buffer
            foreach (KeyValuePair<StreamReader, Queue<string>> readBuffer in readBuffers)
            {
                int currentBufferSize = 0;
                string? line = null;
                StreamReader stream = readBuffer.Key;
                Queue<string> readQueue = readBuffer.Value;

                // fill the read queue which acts as a buffer to decrease I/O
                if (readQueue.Count == 0)
                {
                    while (stream.BaseStream?.CanRead == true && currentBufferSize < bufferSizePerFile && stream.Peek() >= 0)
                    {
                        line ??= stream.ReadLine();

                        if (line == null)
                        {
                            break;
                        }

                        var lineSize = Encoding.ASCII.GetByteCount(line);

                        if (currentBufferSize + lineSize > bufferSizePerFile)
                        {
                            break;
                        }

                        readQueue.Enqueue(line);
                        currentBufferSize += lineSize;
                        line = null;
                    }
                }

                // no more data
                if (stream.BaseStream?.CanRead == true && stream.Peek() == -1)
                {
                    stream.Close();
                }

                // stream closed and read queue is empty so we mark the read buffer to be disposed
                if (stream.BaseStream?.CanRead == false && readQueue.Count == 0)
                {
                    streamsToDelete.Add(stream);
                }
            }

            // remove unnecessary read buffer
            streamsToDelete.ForEach(stream => readBuffers.Remove(stream));

            // fill write queue
            foreach (KeyValuePair<StreamReader, Queue<string>> readBuffer in readBuffers)
            {
                int currentBufferSize = 0;
                string? line = null;
                Queue<string> readQueue = readBuffer.Value;
                if (currentBufferSize < bufferSizePerFile && readQueue.Count > 0)
                {
                    line ??= readQueue.Dequeue();

                    var lineSize = Encoding.ASCII.GetByteCount(line);

                    if (currentBufferSize + lineSize > bufferSizePerFile)
                    {
                        break;
                    }

                    sortedQueue.Enqueue(line);
                    currentBufferSize += lineSize;
                    line = null;
                }
            }

            // write all data from the sorted queue
            SortedQueueNode? node;
            while (sortedQueue.IsEmpty == false && (node = sortedQueue.Dequeue()) != null)
            {
                outputFileStream.WriteLine(node.data.ToFormattedString());
            }

            // quit if nothing left
            if (readBuffers.Count == 0)
            {
                break;
            }
        }
    }
}