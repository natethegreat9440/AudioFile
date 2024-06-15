//Create a IQueue interface for the Queue class. Methods should include Enqueue(), Dequeue(), Peek(), IsEmpty(), and GetQueue() and GenerateQueue()
    
public interface IQueue
{
    //void Enqueue(MediaItem item); //Adds to the end of the queue. Not sure if I need this inheriting ILibrary.Add()
    MediaItem Dequeue(); //Remove and return item at front of queue
    MediaItem Peek(); // Returns first item at front of queue without alterating queue
    void AddUpNext();
    bool IsEmpty();
    List<MediaItem> GetQueue();
    List<Track> GenerateQueue(Func<List<Track>, int, List<Track>> shuffleMethod, int queueLength); 
}