//Create a concrete class called Visualizer that implements the IVisualizer interface. 

using System;
using System.Collections.Generic;

public class Visualizer : IVisualizer
{
    #region Singleton pattern with Lazy<T> implementation (thread-safe)
    private static readonly Lazy<Visualizer> lazy =
        new Lazy<Visualizer>(() => new Visualizer());

    public static Visualizer Instance { get { return lazy.Value; } }

    private List<Visual> visuals;

    private Visualizer()
    {
        visuals = new List<Visual>();
    }
    #endregion

    #region Variables
    public Visual this[int index] { get => visuals[index]; set => visuals[index] = value; }

    public int Count => visuals.Count;

    public bool IsReadOnly => false;
    #endregion

    #region IList implementation

    public void Add(Visual item)
    {
        visuals.Add(item);
    }

    public void Clear()
    {
        visuals.Clear();
    }

    public bool Contains(Visual item)
    {
        return visuals.Contains(item);
    }

    public void CopyTo(Visual[] array, int arrayIndex)
    {
        visuals.CopyTo(array, arrayIndex);
    }

    public int IndexOf(Visual item)
    {
        return visuals.IndexOf(item);
    }

    public void Insert(int index, Visual item)
    {
        visuals.Insert(index, item);
    }

    public bool Remove(Visual item)
    {
        return visuals.Remove(item);
    }

    public void RemoveAt(int index)
    {
        visuals.RemoveAt(index);
    }
    #endregion

    public void Initialize();
    {
        throw new NotImplementedException();
    }    

    public void UpdateVisualization();
    {
       throw new NotImplementedException();     
    }

    public void Reset();
    {
        throw new NotImplementedException();
    }
}