using LineSorterApp.Helpers;

namespace LineSorterApp.DataStructures;

public class SortedQueue
{
    private SortedQueueNode? first;
    private readonly IComparer<Row> rowComparer = new RowComparer();

    public bool IsEmpty => first == null;

    public void Enqueue(string data)
    {
        var newNode = new SortedQueueNode(data);
        var dataRow = data.ToRow();

        if (first == null) {
            first = newNode;
            return;
        }

        if (rowComparer.Compare(first.data, dataRow) > 0) {
            newNode.next = first;
            first.prev = newNode;
            first = newNode;
        } else if (first.next == null) {
            first.next = newNode;
            newNode.prev = first;
        } else {
            QueueIn(first, newNode);
        }
    }

    private void QueueIn(SortedQueueNode root, SortedQueueNode newNode)
    {
        if (rowComparer.Compare(root.data, newNode.data) > 0)
        {
            if (root.prev != null)
            {
                root.prev.next = newNode;
                newNode.prev = root.prev;
            }

            root.prev = newNode;
            newNode.next = root;

            return;
        }

        if (root.next != null) {
            QueueIn(root.next, newNode);
            return;
        }

        root.next = newNode;
        newNode.prev = root;
    }

    public SortedQueueNode? Dequeue() {
        var node = first;
        if (node?.next != null) {
            first = node.next;
        } else {
            first = null;
        }

        return node;
    }
}