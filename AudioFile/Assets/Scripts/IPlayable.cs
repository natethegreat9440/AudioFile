/*Create an IPlayable interface to be implemented by a Track, CDWriter, RecordWriter, Playlist, and Visualizer class. 
/This interface will have a Play method that will be called by the MediaPlayerManager class. It will also have a Pause() method and a Stop() method
*/
public interface IPlayable
{
        void Play();
        void Pause();
        void Stop();
        float GetDuration();
        float GetCurrentPosition();
}

    



