using LineSorterApp.Helpers;

namespace LineSorterApp.DataStructures;

public class QueueNode
    {
        public readonly Row data;

        public readonly StreamReader stream;

        public QueueNode? next;

        public QueueNode? prev;

        public QueueNode(string data, StreamReader stream)
        {
            this.data = data.ToRow();
            this.stream = stream;
        }
    }