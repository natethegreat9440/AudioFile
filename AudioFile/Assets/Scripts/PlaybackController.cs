using System.Collections;
using System.Collections.Generic;
using AudioFile;
using System.Windows.Forms;
using AudioFile.Model;
using AudioFile.View;
using AudioFile.ObserverManager;
using System;
using UnityEngine;


namespace AudioFile.Controller
{
    /// <summary>
    /// Singleton Playback Controller in AudioFile used for controlling playback functions of tracks within TrackLibrary and Playlists
    /// <remarks>
    /// May be modified later to control synchronization of track and visualizer playback. 
    /// Will be modified to pass along all commands to the CommandStackController for undo/redo operations.
    /// Members: CurrentTrack, CreateSingleton(), GetCurentTrackIndex(), HandlePlayPauseButton(), SetCurrentTrack(), Play(), Pause(), Stop(), NextItem(), PreviousItem().
    /// Implements Awake(), Start(), and Update() from MonoBehaviour. Implements AudioFileUpdate() from IAudioFileObserver. Implements HandleRequest() from IController.
    /// This controller has no implementation for IController methods Initialize() or Dispose() (yet).
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IAudioFileObserver"/>
    /// <seealso cref="IController"/>
    /// </summary>

    public class PlaybackController : MonoBehaviour, IController, IAudioFileObserver
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<PlaybackController> _instance = new Lazy<PlaybackController>(CreateSingleton);

        // Private constructor to prevent instantiation
        private PlaybackController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static PlaybackController Instance => _instance.Value;

        public Track CurrentTrack { get; private set; }

        private static PlaybackController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(PlaybackController).Name);

            // Add the PlayBackController component to the GameObject
            return singletonObject.AddComponent<PlaybackController>();
        }

        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        public void Start()
        {
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnCurrentTrackIsDone", this);

        }

        void Update()
        {
            if (CurrentTrack != null && CurrentTrack.IsDone())
            {
                Debug.Log("Track has finished playing.");
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnCurrentTrackIsDone", null);
            }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public int GetCurrentTrackIndex()
        {
            return CurrentTrack != null ? Model.TrackLibrary.Instance.GetTrackIndex(CurrentTrack) : -1;
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
                        if (CurrentTrack != null && currentTrackIndex != playCommand.Index)
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
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnCurrentTrackChanged", null);
            }
            else
            {
                Debug.Log("There is no current track.");
            }
            
        }

        public void Play(int index)
        {
            SetCurrentTrack(Model.TrackLibrary.Instance.GetTrackAtIndex(index));
            TrackLibrary.Instance.Play(index);
        }

        public void Pause(int index)
        {
            TrackLibrary.Instance.Pause(index);
        }

        public void Stop(int index)
        {
            TrackLibrary.Instance.Stop(index);
        }

        public void NextItem()
        {
            TrackLibrary.Instance.NextItem();
        }
        public void PreviousItem()
        {
            TrackLibrary.Instance.PreviousItem();
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
