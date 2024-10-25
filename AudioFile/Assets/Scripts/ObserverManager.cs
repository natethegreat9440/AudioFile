using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioFile.ObserverManager
{
    public interface IObserver
    {
        void Update(string observationType, object data);
    }
    public class ObserverManager
    {
        #region Observation and observer pairs
        // Dictionary where the key is the observation type and the value is a list of observers
        private Dictionary<string, List<IObserver>> observers = new Dictionary<string, List<IObserver>>()
        {
            //Uncomment dict items as the classes implement the IObserver interface are created
            //{"OnProgramStart", new List<IObserver>() { TrackListDisplay } }
            //{"OnTrackLibraryUpdate", new List<IObserver>() { TrackListDisplay } }
            //{"OnQueueUpdate", new List<IObserver>() { QueueDisplay } }
            //{"OnCurrentTrackUpdate", new List<IObserver>() { NowPlayingDisplay, QueueDisplay } }
            //{"OnTrackPropertyUpdate", new List<IObserver>() { NowPlayingDisplay, TrackListDisplay, QueueDisplay, SampleDisplay, TabDisplay, LyricsDisplay } }
            //{"OnTrackLibraryFilter", new List<IObserver>() { TrackListDisplay } }
        };
        
        #endregion
        // Register an observer for a specific observation type
        public void RegisterObserver(string observationType, IObserver observer)
        {
            if (!observers.ContainsKey(observationType))
            {
                observers[observationType] = new List<IObserver>();
            }
            observers[observationType].Add(observer);
        }

        // Remove an observer for a specific observation type
        public void RemoveObserver(string observationType, IObserver observer)
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
                    observer.Update(observationType, data);
                }
            }
        }
    }
}
