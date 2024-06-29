//Create a concrete class called Queue that implements the IQueue interface. Make this class a singleton.

using system;
using system.collections;

public class Queue : IQueue<MediaItem>
{
    #region Singleton pattern with Lazy<T> implementation (thread-safe)
    private static readonly Lazy<Queue> lazy =
        new Lazy<Queue>(() => new Queue());

    public static Queue Instance { get { return lazy.Value; } }

    private List<MediaItem> queue;

    private Queue()
    {
        queue = new List<MediaItem>();
    }
    #endregion

    #region Variables
    public MediaItem this[int index] { get => queue[index]; set => queue[index] = value; }

    public int Count => queue.Count;

    public bool IsReadOnly => false;
    #endregion

    #region IList implementation

    public void Add(MediaItem item)
    {
        queue.Add(item);
    }

    public void Clear()
    {
        queue.Clear();
    }

    public bool Contains(MediaItem item)
    {
        return queue.Contains(item);
    }

    public void CopyTo(MediaItem[] array, int arrayIndex)
    {
        queue.CopyTo(array, arrayIndex);
    }

    public int IndexOf(MediaItem item)
    {
        return queue.IndexOf(item);
    }

    public void Insert(int index, MediaItem item)
    {
        queue.Insert(index, item);
    }
    #endregion

    #region IQueue implementation  
    public MediaItem Dequeue()
    {
        if (queue.Count == 0)
        {
            return null;
        }
        MediaItem item = queue[0];
        queue.RemoveAt(0);
        return item;
    }

    public MediaItem Peek()
    {
        if (queue.Count == 0)
        {
            return null;
        }
        return queue[0];
    }

    public void AddUpNext()
    {
        throw new NotImplementedException();
    }

    public List<MediaItem> GetQueue()
    {
        return queue;
    }

    public List<Track> GenerateQueue(Func<List<Track>, int, List<Track>> shuffleMethod, int queueLength)
    {
        throw new NotImplementedException();
    }
    #endregion
}