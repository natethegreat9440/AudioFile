using System.Collections;
using System.Collections.Generic;
using AudioFile;
using System.Windows.Forms;
using AudioFile.Model;
using AudioFile.View;
using AudioFile.ObserverManager;
using System;
using UnityEngine;
using Mono.Data.Sqlite;
using Unity.VisualScripting.Dependencies.Sqlite;
using Unity.VisualScripting.Antlr3.Runtime.Collections;



namespace AudioFile.Controller
{
    /// <summary>
    /// Singleton Playback Controller in AudioFile used for controlling playback functions of tracks within TrackLibrary and Playlists
    /// <remarks>
    /// May be modified later to control synchronization of track and visualizer playback. 
    /// Will be modified to pass along all commands to the CommandStackController for undo/redo operations.
    /// Members: ActiveTrack, SelectedTrack, CreateSingleton(), GetCurentTrackIndex(), HandlePlayPauseButton(), SetActiveTrack(), SetSelectedTrack(), Play(), Pause(), Stop(), NextItem(), PreviousItem().
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

        public Track ActiveTrack { get; private set; } = null;

        public Track SelectedTrack { get; private set; } = null;

        private string CurrentSQLQueryInject => SortController.Instance.CurrentSQLQueryInject;

        public int ActiveTrackIndex => TrackLibraryController.Instance.GetTrackIndex(ActiveTrack.TrackID);
        //{
        //    get { return TrackLibrary.Instance.ActiveTrackIndex; }
        //    set { TrackLibrary.Instance.ActiveTrackIndex = value; }
        //}

        public string ConnectionString => SetupController.Instance.ConnectionString;

        private static PlaybackController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(PlaybackController).Name);

            // Add the PlayBackController component to the GameObject
            return singletonObject.AddComponent<PlaybackController>();
        }

        public void Start()
        {
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnSingleTrackSelected", this);
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnActiveTrackIsDone", this);

        }

        void Update()
        {
            if (ActiveTrack != null && ActiveTrack.IsDone())
            {
                Debug.Log("Track has finished playing.");
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackIsDone", null);
            }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public int GetSelectedTrackID()
        {
            return SelectedTrack != null ? SelectedTrack.TrackID : -1;
        }

        public void HandlePlayPauseButton(int trackDisplayID)
        {
            if (ActiveTrack == null)
            {
                // Play the selected track
                HandleRequest(new PlayCommand(trackDisplayID));
            }
            else if (ActiveTrack != null && ActiveTrack.TrackID == trackDisplayID && SelectedTrack.IsPlaying) //TODO: see if third condition is necessary or if it is just redundant
            {
                // Pause the active track
                HandleRequest(new PauseCommand(trackDisplayID));
            }
            else
            {
                // Play the active/selected track
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
                        PlayCommand playCommand = request as PlayCommand;
                        if (ActiveTrack != null)
                        {
                            if (ActiveTrack.TrackID != playCommand.TrackDisplayID)
                            {
                                Stop();
                            }
                        }
                        Play(playCommand.TrackDisplayID);
                    },
                    "PauseCommand" => () =>
                    {
                        PauseCommand pauseCommand = request as PauseCommand;
                        Pause();
                    },
                    "StopCommand" => () =>
                    {
                        StopCommand stopCommand = request as StopCommand;
                        Stop();
                    },
                    "NextItemCommand" => () => { NextItem(); },
                    "PreviousItemCommand" => () => { PreviousItem(); },
                    "SeekCommand" => () =>
                    {
                        SeekCommand seekCommand = request as SeekCommand;
                        Seek(seekCommand.NewTime);
                    },
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
                        Pause();
                    },
                    "PauseCommand" => () =>
                    {
                        PauseCommand undoPauseCommand = request as PauseCommand;
                        Play(undoPauseCommand.TrackDisplayID);
                    },
                    "NextItemCommand" => () => { PreviousItem(); },
                    "PreviousItemCommand" => () => { NextItem(); },
                    "SeekCommand" => () => 
                    {
                        SeekCommand seekCommand = request as SeekCommand;
                        Seek(seekCommand.PreviousTime);
                    },
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

        public void SetActiveTrack(Track track)
        {
            if (track != null)
            {
                ActiveTrack = track;
                Debug.Log($"Active track set to: {ActiveTrack}");
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackChanged", null);
            }
            else
            {
                Debug.Log("There is no active track.");
            }
        }

        public void SetSelectedTrack(Track track)
        {
            if (track != null)
            {
                SelectedTrack = track;
                Debug.Log($"Selected track set to: {SelectedTrack}");
                //ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackChanged", null);
            }
            else
            {
                Debug.Log("There is no selected track.");
            }
        }

        public void Play(int trackDisplayID)
        {
            SetActiveTrack(TrackLibraryController.Instance.GetTrackAtID(trackDisplayID));

            try
            {
                ActiveTrack.Play();
            }
            catch (Exception ex)
            {
                Debug.Log($"Track skipped: {ex.GetType()} - {ex.Message}");
                Skip();
            }
        }

        public void Pause()
        {
            ActiveTrack.Pause();
        }

        public void Stop()
        {
            ActiveTrack.Stop();
        }

        public bool NextItem()
        {
            //int activeTrackIndex = TrackLibraryController.Instance.GetTrackIndex(ActiveTrack.TrackID); //Testing

            int nextTrackIndex = TrackLibraryController.Instance.GetTrackIndex(ActiveTrack.TrackID, true); //True indicates to grab the index of next track in table
            int tracksLength = TrackLibraryController.Instance.GetTracksLength(); 

            if (nextTrackIndex < tracksLength + 1)
            {
                Stop();

                int nextTrackID = TrackLibraryController.Instance.GetTrackIDAtIndex(nextTrackIndex);
                Track nextTrack = TrackLibraryController.Instance.GetTrackAtID(nextTrackID);
                SetActiveTrack(nextTrack);

                ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackCycled", ActiveTrackIndex);
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackChanged", null);
                
                Play(nextTrackID);
                return true;
            }
            else
            {
                Debug.Log("Reached the end of the playlist.");
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackListEnd", ActiveTrackIndex);
                return false;
            }
        }
        public void PreviousItem()
        {
            int prevTrackIndex = TrackLibraryController.Instance.GetTrackIndex(ActiveTrack.TrackID, false, true); //False and then true indicates to grab the index of previous track in table

            if (prevTrackIndex > 0)
            {
                Stop();

                int prevTrackID = TrackLibraryController.Instance.GetTrackIDAtIndex(prevTrackIndex);
                Track prevTrack = TrackLibraryController.Instance.GetTrackAtID(prevTrackID);
                SetActiveTrack(prevTrack);

                ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackCycled", ActiveTrackIndex);
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackChanged", null);

                Play(prevTrackID);
            }
            else
            {
                Debug.Log("Reached the front of the playlist.");
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackListEnd", ActiveTrackIndex);
            }
        }

        public void Skip() //Commenting this out for now as it seems to me this Skip() logic could just be implemented into the Play() method directly 
        //(and potentially other playback methods, however I'll want to test how Skip logic works inside Play before deciding if I want to add to other methods)
        {
            if (ActiveTrack != null && !NextItem())
            {
                Debug.Log("Delegating to PreviousItem() because the end of the playlist was reached.");
                PreviousItem(); // Fallback to previous item
            }

            ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackSkipped", ActiveTrack); //Passes the track name if possible when skipped
        }

        public void Seek(float newTime)
        {
            ActiveTrack.SetTime(newTime);
        }

        public float GetTime()
        {
            return ActiveTrack.GetTime();
        }
        public void AudioFileUpdate(string observationType, object data)
        {
            //Debug.Log(observationType);
            Action action = observationType switch
            {
                "OnActiveTrackIsDone" => () => { NextItem(); },
                "OnSingleTrackSelected" => () =>
                {
                    SelectedTrack = TrackLibraryController.Instance.GetTrackAtID((int)data);
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }
    }
}
