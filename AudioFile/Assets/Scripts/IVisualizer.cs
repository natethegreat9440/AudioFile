//Allow for IVisualizer to implement IPlayable interface
//Add the necessary methods to IVisualizer to implement the IPlayable interface
using System;
using System.Collections;
using System.Collections.Generic;
public interface IVisualizer<T> : IPlayable<T>
{
    void Initialize();
    void UpdateVisualization();
    void Reset();
    
}