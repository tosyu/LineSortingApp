namespace LineSorterApp.DataStructures;

public class QueueNode
    {
        public readonly string data;

        public readonly StreamReader stream;

        public QueueNode? next;

        public QueueNode? prev;

        public QueueNode(string data, StreamReader stream)
        {
            this.data = data;
            this.stream = stream;
        }
    }