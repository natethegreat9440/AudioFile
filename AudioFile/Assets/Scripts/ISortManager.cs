using System.Collections.Generic;

public interface ISortManager<T>
{
    void Sort(IList<T> items, IComparer<T> comparer);
    // Additional sorting methods as needed
}