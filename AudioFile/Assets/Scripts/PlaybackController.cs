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
    public class PlaybackController : MonoBehaviour, IController
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<PlaybackController> _instance = new Lazy<PlaybackController>(() => new PlaybackController());

        // Private constructor to prevent instantiation
        private PlaybackController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static PlaybackController Instance => _instance.Value;

        private Track _currentTrack;

        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        public void Start()
        {

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
            _currentTrack = track;
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
            throw new System.NotImplementedException();
        }
        public void PreviousItem()
        {
            throw new System.NotImplementedException();
        }

        /* TODO: Move this to an exitprogram controller
        public void ExitProgram()
        {
            throw new System.NotImplementedException();
        }
        */
    }
}
