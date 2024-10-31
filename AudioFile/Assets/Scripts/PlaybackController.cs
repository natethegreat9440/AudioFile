using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioFile;
using AudioFile.Model;
using AudioFile.View;
using AudioFile.ObserverManager;
using System;
using System.Windows.Forms;

namespace AudioFile.Controller
{
    public class PlaybackController : MonoBehaviour, IController, IAudioFileObserver
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<PlaybackController> _instance = new Lazy<PlaybackController>(CreateSingleton);

        // Private constructor to prevent instantiation
        private PlaybackController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static PlaybackController Instance => _instance.Value;

        private Track _currentTrack;

        private static PlaybackController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(PlaybackController).Name);

            // Add the TrackListController component to the GameObject
            return singletonObject.AddComponent<PlaybackController>();
        }

        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        public void Start()
        {
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnCurrentTrackIsDone", this);

        }

        void Update()
        {
            if (_currentTrack != null && _currentTrack.IsDone())
            {
                Debug.Log("Track has finished playing.");
                AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnCurrentTrackIsDone", null);
            }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void HandleRequest(object request, bool isUndo)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void SetCurrentTrack(Track track)
        {
            if (track != null)
            {
                _currentTrack = track;
                Debug.Log($"Current track set to: {_currentTrack}");
            }
            else
            {
                Debug.Log("There is no current track.");
            }
            
        }

        public void Play()
        {
            throw new System.NotImplementedException();
        }

        public void Pause()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public void NextItem()
        {
            AudioFile.Model.TrackLibrary.Instance.NextItem();
        }
        public void PreviousItem()
        {
            throw new System.NotImplementedException();
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            switch (observationType)
            {
                case "OnCurrentTrackIsDone":
                    NextItem();
                    break;
                // Add more cases here if needed
                default:
                    Debug.LogWarning($"Unhandled observation type: {observationType}");
                    break;
            }
        }

        /* TODO: Move this to an exitprogram controller
        public void ExitProgram()
        {
            throw new System.NotImplementedException();
        }
        */
    }
}
