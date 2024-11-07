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

        public Track CurrentTrack { get; private set; }

        public bool isPlaying = false;

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
            //Should this be coroutine?
            if (CurrentTrack != null && CurrentTrack.IsDone())
            {
                Debug.Log("Track has finished playing.");
                AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnCurrentTrackIsDone", null);
            }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public int GetCurrentTrackIndex()
        {
            return CurrentTrack != null ? AudioFile.Model.TrackLibrary.Instance.GetTrackIndex(CurrentTrack) : -1;
        }

        public void HandlePlayPauseButton(int trackDisplayIndex)
        {
            if (CurrentTrack == null)
            {
                // Play the selected track
                HandleRequest(new PlayCommand(trackDisplayIndex));
            }
            else if (CurrentTrack != null && GetCurrentTrackIndex() == trackDisplayIndex && CurrentTrack.IsPlaying)
            {
                // Pause the current track
                HandleRequest(new PauseCommand(trackDisplayIndex));
            }
            else
            {
                // Play the selected track
                HandleRequest(new PlayCommand(trackDisplayIndex));
            }
        }
        public void HandleRequest(object request, bool isUndo = false)
        {
            //Add methods to log these commands with the UndoController
            string command = request.GetType().Name;

            if (isUndo == false)
            {
                switch (command)
                {
                    case "PlayCommand":
                        int currentTrackIndex = GetCurrentTrackIndex();
                        PlayCommand playCommand = request as PlayCommand;
                        if (CurrentTrack != null && currentTrackIndex != playCommand.Index)//(CurrentTrack.IsPlaying || CurrentTrack.IsPaused))
                        {
                            Stop(currentTrackIndex);
                        }
                        Play(playCommand.Index);
                        break;

                    case "PauseCommand":
                        PauseCommand pauseCommand = request as PauseCommand;
                        Pause(pauseCommand.Index);
                        break;

                    case "StopCommand":
                        StopCommand stopCommand = request as StopCommand;
                        Stop(stopCommand.Index);
                        break;

                    case "NextItemCommand":
                        NextItem();
                        break;

                    case "PreviousItemCommand":
                        PreviousItem();
                        break;

                    default:
                        Debug.LogWarning($"Unhandled command: {request}");
                        break;
                }
            }
            else
            {
                switch (command)
                {
                    case "PlayCommand":
                        PlayCommand undoPlayCommand = request as PlayCommand;
                        Pause(undoPlayCommand.Index);
                        break;
                    case "PauseCommand":
                        PauseCommand undoPauseCommand = request as PauseCommand;
                        Play(undoPauseCommand.Index);
                        break;
                    case "NextItemCommand":
                        PreviousItem();
                        break;
                    case "PreviousItemCommand":
                        NextItem();
                        break;
                    default:
                        Debug.LogWarning($"Unhandled undo command: {request}");
                        break;
                }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void SetCurrentTrack(Track track)
        {
            if (track != null)
            {
                CurrentTrack = track;
                Debug.Log($"Current track set to: {CurrentTrack}");
                AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnCurrentTrackChanged", null);
            }
            else
            {
                Debug.Log("There is no current track.");
            }
            
        }

        public void Play(int index)
        {
            SetCurrentTrack(AudioFile.Model.TrackLibrary.Instance.GetTrackAtIndex(index));
            AudioFile.Model.TrackLibrary.Instance.Play(index);
            //isPlaying = true;
        }

        public void Pause(int index)
        {
            AudioFile.Model.TrackLibrary.Instance.Pause(index);
            //isPlaying = false;
        }

        public void Stop(int index)
        {
            AudioFile.Model.TrackLibrary.Instance.Stop(index);
            //isPlaying = false;
        }

        public void NextItem()
        {
            AudioFile.Model.TrackLibrary.Instance.NextItem();
            //isPlaying = true;
        }
        public void PreviousItem()
        {
            AudioFile.Model.TrackLibrary.Instance.PreviousItem();
            //isPlaying = true;
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
                    Debug.LogWarning($"Unhandled observation type: {observationType} at {this}");
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
