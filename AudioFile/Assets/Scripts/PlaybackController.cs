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

        public Track CurrentTrack { get; private set; } = null;

        private static PlaybackController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(PlaybackController).Name);

            // Add the PlayBackController component to the GameObject
            return singletonObject.AddComponent<PlaybackController>();
        }

        /*public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }*/

        public void Start()
        {
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackSelected", this);
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

        public string GetCurrentTrackID()
        {
            return CurrentTrack != null ? Model.TrackLibrary.Instance.GetTrackID(CurrentTrack) : "";
        }


        public void HandlePlayPauseButton(string trackDisplayID)
        {
            if (CurrentTrack == null)
            {
                // Play the selected track
                HandleRequest(new PlayCommand(trackDisplayID));
            }
            else if (CurrentTrack != null && GetCurrentTrackID() == trackDisplayID && CurrentTrack.IsPlaying)
            {
                // Pause the current track
                HandleRequest(new PauseCommand(trackDisplayID));
            }
            else
            {
                // Play the selected track
                HandleRequest(new PlayCommand(trackDisplayID));
            }
        }
        public void HandleRequest(object request, bool isUndo = false)
        {
            //Add methods to log these commands with the UndoController
            string command = request.GetType().Name;

            if (isUndo == false)
            {
                Action action = command switch
                {
                    "PlayCommand" => () =>
                    {
                        string currentTrackID = GetCurrentTrackID();
                        PlayCommand playCommand = request as PlayCommand;
                        if (CurrentTrack != null && currentTrackID != playCommand.TrackDisplayID)
                        {
                            Stop(currentTrackID);
                        }
                        Play(playCommand.TrackDisplayID);
                    },
                    "PauseCommand" => () =>
                    {
                        PauseCommand pauseCommand = request as PauseCommand;
                        Pause(pauseCommand.TrackDisplayID);
                    },
                    "StopCommand" => () =>
                    {
                        StopCommand stopCommand = request as StopCommand;
                        Stop(stopCommand.TrackDisplayID);
                    },
                    "NextItemCommand" => NextItem,
                    "PreviousItemCommand" => PreviousItem,
                    //Add more switch arms here as needed
                    _ => () => Debug.LogWarning($"Unhandled command: {request}")
                };

                action();
            }
            else
            {
                Action action = command switch
                {
                    "PlayCommand" => () =>
                    {
                        PlayCommand undoPlayCommand = request as PlayCommand;
                        Pause(undoPlayCommand.TrackDisplayID);
                    },
                    "PauseCommand" => () =>
                    {
                        PauseCommand undoPauseCommand = request as PauseCommand;
                        Play(undoPauseCommand.TrackDisplayID);
                    },
                    "NextItemCommand" => PreviousItem,
                    "PreviousItemCommand" => NextItem,
                    //Add more switch arms here as needed
                    _ => () => Debug.LogWarning($"Unhandled undo command: {request}")
                };

                action();
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

        public void Play(string trackDisplayID)
        {
            SetCurrentTrack(TrackLibrary.Instance.GetTrackAtID(trackDisplayID));
            TrackLibrary.Instance.Play(trackDisplayID);
        }

        public void Pause(string trackDisplayID)
        {
            TrackLibrary.Instance.Pause(trackDisplayID);
        }

        public void Stop(string trackDisplayID)
        {
            TrackLibrary.Instance.Stop(trackDisplayID);
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
            Debug.Log(observationType);
            Action action = observationType switch
            {
                "OnCurrentTrackIsDone" => NextItem,
                "OnTrackSelected" => () =>
                {
                    Debug.Log($"CurrentTrack = {CurrentTrack}");
                    if (CurrentTrack == null)
                    {
                        Debug.Log("Setting current track to: " + (string)data);
                        SetCurrentTrack(TrackLibrary.Instance.GetTrackAtID((string)data));
                    }
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }

        /* TODO: Move this to an exitprogram controller
        public void ExitProgram()
        {
            throw new System.NotImplementedException();
        }
        */
    }
}
