using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioFile.ObserverManager;
using AudioFile.Model;
using AudioFile.View;
using AudioFile.Controller;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace AudioFile.ObserverManager
{
    /// <summary>
    /// Service layer for the concrete singleton service layer class ObserverManager and the interface IAudioFileObserver
    /// <remarks>
    /// ObserverManager utilizes an Observer design pattern and encapsulates all Observation Types into a single Dictionary object to simplify and cut down on the 
    /// amound of code needed to add and remove observers for different events (as in a typical Observer design pattern).
    /// Events are now ObservationType strings passed to the methods in ObserverManager. IMPORTANT NOTE: Initialize singletons that register as observers in SetupController.
    /// Members: NotifyObservers(), RegisterObserver(), RemoveObserver(), CheckObservers(). Latter two are used dynamically at runtime.
    /// Registering an object as an observer is as simple as calling RegisterObserver() in a Start() method (typically) and implementing IAudioFileObserver.
    /// Subjects notifying Observers is as simple as calling NotifyObservers() and passing the observation type as a string.
    /// Observers implementing the IAudioFileObserver interface use the lone interface method AudioFileUpdate() for processing various ObservationTypes usually with switch statements
    /// if the Observer is interested in a multiple different ObservationTypes. In context of the AudioFile application typically Model layer objects are Subjects and View layer objects are Observers
    /// (and sometimes Controllers can be both) in accordance with an MVC design pattern. For primarily debugging purposes a CheckObservers() method is available to see which observers are registered to a given observation type.
    /// </remarks>
    /// <see cref="IAudioFileObserver"/>
    /// <see also cref="ObserverManager"/>
    /// </summary>

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
                //These comments represent a sample (not frequently updated) of the different ObservationTypes and Observers
                //It is simpler for initialization purposes to have objects as observers dynamically and tracking program flow at this time than to try to define all of the 
                //relationships at the same time here
                //For debugging purposes call CheckObservers() to see which Observers are registered to a given Observation type or just use Find All in solution in Visual Studio
                //If something is uncommented out that just means I am doing initial debugging and testing


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

        public void CheckObservers(string observationType)
        {
            if (observers.ContainsKey(observationType))
            {
                List<IAudioFileObserver> observerList = observers[observationType];
                foreach (var observer in observerList)
                {
                    Debug.Log(observationType + " has " + observer.ToString());
                }
            }
            else
            {
                Debug.Log($"No observers found for observation type: {observationType}");
            }
        }
    }
}
