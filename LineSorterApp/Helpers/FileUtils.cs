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
        var readBuffers = new Dictionary<StreamReader, Queue<string>>();
        bool readBuffersEmpty;
        var streamsToDelete = new List<StreamReader>();
        var rowComparer = new RowComparer();
        int lineSize;
        int currentBufferSize;
        string? line;
        StreamReader stream;
        Queue<string> readQueue;

        // setup buffers
        inputFiles.ForEach(file => readBuffers.Add(new StreamReader(file.FullName, Encoding.ASCII, false), []));

        while (true)
        {
            // loop that will either fill the read queue or enqueue the sorted queue or close the buffer
            foreach (KeyValuePair<StreamReader, Queue<string>> readBuffer in readBuffers)
            {
                currentBufferSize = 0;
                line = null;
                stream = readBuffer.Key;
                readQueue = readBuffer.Value;

                // fill the read queue which acts as a buffer to decrease I/O
                if (readQueue.Count == 0)
                {
                    lineSize = 0;
                    while (stream.BaseStream?.CanRead == true && currentBufferSize < bufferSizePerFile && stream.Peek() >= 0)
                    {
                        line ??= stream.ReadLine();

                        if (line == null)
                        {
                            break;
                        }

                        lineSize = Encoding.ASCII.GetByteCount(line);

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
            currentBufferSize = 0;
            line = null;
            lineSize = 0;
            var outputBuffer = new List<Row>();
            var writeBufferFilled = false;

            while (writeBufferFilled == false)
            {                
                readBuffersEmpty = true;
                foreach (KeyValuePair<StreamReader, Queue<string>> readBuffer in readBuffers)
                {
                    readQueue = readBuffer.Value;

                    if (readQueue.Count == 0)
                    {
                        continue;
                    }

                    readBuffersEmpty = false;

                    line ??= readQueue.Dequeue();

                    lineSize = Encoding.ASCII.GetByteCount(line);                   

                    outputBuffer.Add(line.ToRow());
                    currentBufferSize += lineSize;
                    line = null;

                    if (currentBufferSize >= bufferSizePerFile)
                    {
                        writeBufferFilled = true;
                        break;
                    }
                }

                if (readBuffersEmpty)
                {
                    writeBufferFilled = true;
                }
            }

            outputBuffer.Sort(rowComparer);
            outputBuffer.AppendAllRows(outputFileStream);

            // quit if nothing left
            if (readBuffers.Count == 0)
            {
                break;
            }
        }
    }
}