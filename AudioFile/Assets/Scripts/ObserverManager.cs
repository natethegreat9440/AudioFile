using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioFile.ObserverManager;
using AudioFile.Model;
using AudioFile.View;
using AudioFile.Controller;

namespace AudioFile.ObserverManager
{
    public interface IAudioFileObserver
    {
        void AudioFileUpdate(string observationType, object data);
    }
    public class ObserverManager
    {
        
        private static readonly Lazy<ObserverManager> lazyInstance = new Lazy<ObserverManager>(() => new ObserverManager());
        // Dictionary where the key is the observation type and the value is a list of observers
        private Dictionary<string, List<IAudioFileObserver>> observers;
        // Private constructor to prevent direct instantiation
        private ObserverManager()
        {
            observers = new Dictionary<string, List<IAudioFileObserver>>()
            #region Observation and observer pairs
            {
                //These comments represent how the dict items should look as the classes implement the IAudioFileObserver interface are created
                //And then added to the list using the RegisterObserver method in startup scripts
                //If somethingn is uncommented out that just means I am doing initial debugging and testing

                //{"OnProgramStart", new List<IAudioFileObserver>() { TrackListDisplay } }
                //{"OnTrackAdded", new List<IAudioFileObserver>() { _UITrackListDisplayManager } },
                //{"OnTrackRemoved", new List<IAudioFileObserver>() { _UITrackListDisplayManager } },
                //{"OnQueueUpdate", new List<IAudioFileObserver>() { QueueDisplay } }
                //{"OnCurrentTrackIsDone", new List<IAudioFileObserver>() {  new TrackLibrary() } } //, NowPlayingDisplay, QueueDisplay} }
                //{"OnTrackPropertyUpdate", new List<IAudioFileObserver>() { NowPlayingDisplay, TrackListDisplay, QueueDisplay, SampleDisplay, TabDisplay, LyricsDisplay } }
                //{"OnTrackLibraryFilter", new List<IAudioFileObserver>() { TrackListDisplay } }
            };
            #endregion
        }
        // Public property to access the singleton instance
        public static ObserverManager Instance
        {
            get
            {
                return lazyInstance.Value;
            }
        }

        // Register an observer for a specific observation type
        public void RegisterObserver(string observationType, IAudioFileObserver observer)
        {
            if (!observers.ContainsKey(observationType))
            {
                observers[observationType] = new List<IAudioFileObserver>();
            }
            observers[observationType].Add(observer);
        }

        // Remove an observer for a specific observation type
        public void RemoveObserver(string observationType, IAudioFileObserver observer)
        {
            if (observers.ContainsKey(observationType))
            {
                observers[observationType].Remove(observer);
                // If the list is empty, remove the key from the dictionary
                if (observers[observationType].Count == 0)
                {
                    observers.Remove(observationType);
                }
            }
        }

        // Notify all observers about an update for a specific observation type
        public void NotifyObservers(string observationType, object data = null)
        {
            if (observers.ContainsKey(observationType))
            {
                foreach (var observer in observers[observationType])
                {
                    observer.AudioFileUpdate(observationType, data);
                }
            }
        }
    }
}
