//Create a IQueue interface for the Queue class. Methods should include Enqueue(), Dequeue(), Peek(), IsEmpty(), and GetQueue() and GenerateQueue()

using System;
using System.Collections;
using System.Collections.Generic;

public interface IQueue<T>
{
    void Enqueue(T item); //Adds to the end of the queue. Not sure if I need this inheriting ILibrary.Add()
    T Dequeue(); //Remove and return item at front of queue
    T Peek(); // Returns first item at front of queue without alterating queue
    void AddUpNext();
    //bool IsEmpty();
    List<T> GetQueue();
    List<T> GenerateQueue(Func<List<T>, int, List<T>> shuffleMethod, int queueLength); 
}