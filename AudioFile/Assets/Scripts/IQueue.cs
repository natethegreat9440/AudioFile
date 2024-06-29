//Create a IQueue interface for the Queue class. Methods should include Enqueue(), Dequeue(), Peek(), IsEmpty(), and GetQueue() and GenerateQueue()

using System;
using System.Collections;
using System.Collections.Generic;

public interface IQueue<T> where T : Track
{
    void Enqueue(Track item); //Adds to the end of the queue. Not sure if I need this inheriting ILibrary.Add()
    Track Dequeue(); //Remove and return item at front of queue
    Track Peek(); // Returns first item at front of queue without alterating queue
    void AddUpNext();
    //bool IsEmpty();
    List<Track> GetQueue();
    List<Track> GenerateQueue(Func<List<Track>, int, List<Track>> shuffleMethod, int queueLength); 
}