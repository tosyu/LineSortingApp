using LineSorterApp.Helpers;

namespace LineSorterApp.DataStructures;

public class SortedQueueNode(string data)
{
        public readonly Row data = data.ToRow();

        public SortedQueueNode? next;

        public SortedQueueNode? prev;
}