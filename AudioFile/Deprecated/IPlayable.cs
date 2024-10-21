/*Create an IPlayable interface to be implemented by a Track, CDWriter, RecordWriter, Playlist, and Visualizer class. 
/This interface will have a Play method that will be called by the MediaPlayerManager class. It will also have a Pause() method and a Stop() method
*/
using System;
using System.Collections;
using System.Collections.Generic;
public interface IPlayable<T>
{
        void Play(T item);
        void Pause(T item);
        void Stop(T item);
        float GetDuration(T item);
        float GetCurrentPosition(T item);
}

    



