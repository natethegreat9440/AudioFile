//Create a concrete class called VisualLibrary that implements the ILibrary interface. Make this class a singleton.

using System;

public class VisualizerLibrary : ILibrary<Visualizer>
{
    #region Singleton pattern with Lazy<T> implementation (thread-safe)
    private static readonly Lazy<VisualizerLibrary> lazy =
        new Lazy<VisualizerLibrary>(() => new VisualizerLibrary());

    public static VisualizerLibrary Instance { get { return lazy.Value; } }

    private List<Visualizer> visuals;

    private VisualizerLibrary()
    {
        visuals = new List<Visualizer>();
    }
    #endregion

    #region Variables
    public Visualizer this[int index] { get => visuals[index]; set => visuals[index] = value; }

    public int Count => visuals.Count;

    public bool IsReadOnly => false;
    #endregion

    #region IList implementation

    public void Add(Visualizer item)
    {
        visuals.Add(item);
    }

    public void Clear()
    {
        visuals.Clear();
    }

    public bool Contains(Visualizer item)
    {
        return visuals.Contains(item);
    }

    public void CopyTo(Visualizer[] array, int arrayIndex)
    {
        visuals.CopyTo(array, arrayIndex);
    }

    public int IndexOf(Visualizer item)
    {
        return visuals.IndexOf(item);
    }

    public void Insert(int index, Visualizer item)
    {
        visuals.Insert(index, item);
    }

    public bool Remove(Visualizer item)
    {
        return visuals.Remove(item);
    }

    public void RemoveAt(int index)
    {
        visuals.RemoveAt(index);
    }

    #endregion
}