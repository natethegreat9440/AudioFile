//Allow for IVisualizer to implement IPlayable interface
//Add the necessary methods to IVisualizer to implement the IPlayable interface
public interface IVisualizer : IPlayable
{
    void Initialize();
    void UpdateVisualization();
    void Reset();
    
}