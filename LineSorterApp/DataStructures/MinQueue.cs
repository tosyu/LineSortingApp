using LineSorterApp.Helpers;

namespace LineSorterApp.DataStructures;

public class MinQueue
{
    private QueueNode? first;
    private readonly IComparer<string> lineComparer = new LineComparer();

    public bool IsEmpty => first == null;

    public void Queue(string data, StreamReader associatedStream)
    {
        var newNode = new QueueNode(data, associatedStream);

        if (first == null) {
            first = newNode;
            return;
        }

        if (lineComparer.Compare(first.data, data) > 0) {            
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

    private void QueueIn(QueueNode root, QueueNode newNode)
    {
        if (lineComparer.Compare(root.data, newNode.data) > 0)
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

    public QueueNode? Dequeue() {
        var node = first;
        if (node?.next != null) {
            first = node.next;
        } else {
            first = null;
        }

        return node;
    }
}