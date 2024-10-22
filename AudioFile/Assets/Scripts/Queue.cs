//Create a concrete class called Queue that implements the IQueue interface. Make this class a singleton.
/*
using System;
using System.Collections;
using System.Collections.Generic;

public class Queue<T> : IQueue<T> where T : IPlayable<T>
{
    #region Singleton pattern with Lazy<T> implementation (thread-safe)
    private static readonly Lazy<Queue<T>> lazy =
        new Lazy<Queue<T>>(() => new Queue<T>());

    public static Queue<T> Instance { get { return lazy.Value; } }

    private List<T> queue;

    private Queue()
    {
        queue = new List<T>();
    }
    #endregion

    #region Variables
    public T this[int index] { get => queue[index]; set => queue[index] = value; }

    public int Count => queue.Count;

    public bool IsReadOnly => false;
    #endregion

    #region IList implementation

    public void Add(T item)
    {
        queue.Add(item);
    }

    public void Clear()
    {
        queue.Clear();
    }

    public bool Contains(T item)
    {
        return queue.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        queue.CopyTo(array, arrayIndex);
    }

    public int IndexOf(T item)
    {
        return queue.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        queue.Insert(index, item);
    }
    #endregion

    #region IQueue implementation  
    public T Dequeue()
    {
        if (queue.Count == 0)
        {
            return default(T);
        }
        T item = queue[0];
        queue.RemoveAt(0);
        return item;
    }

    public T Peek()
    {
        if (queue.Count == 0)
        {
            return default(T);
        }
        return queue[0];
    }

    public void Enqueue(T item)
    {
        queue.Add(item);
    }

    public void AddUpNext()
    {
        throw new NotImplementedException();
    }

    public List<T> GetQueue()
    {
        return queue;
    }

    public List<T> GenerateQueue(Func<List<T>, int, List<T>> shuffleMethod, int queueLength)
    {
        throw new NotImplementedException();
    }
    #endregion
}*/