﻿using System.Collections;
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

        public int ActiveTrackIndex
        {
            get { return TrackLibrary.Instance.ActiveTrackIndex; }
            set { TrackLibrary.Instance.ActiveTrackIndex = value; }
        }

        List<Track> TrackList 
        {
            get { return TrackLibrary.Instance.TrackList; }
        }

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

        public string GetSelectedTrackID()
        {
            return SelectedTrack != null ? Model.TrackLibrary.Instance.GetTrackID(SelectedTrack) : "";
        }

        private string GetActiveTrackID()
        {
            foreach (var track in TrackList)
            {
                if (track.IsPlaying || track.IsPaused)
                {
                    return Model.TrackLibrary.Instance.GetTrackID(track);
                }
            }
            return "";
        }

        public void HandlePlayPauseButton(string trackDisplayID)
        {
            if (ActiveTrack == null)
            {
                // Play the selected track
                HandleRequest(new PlayCommand(trackDisplayID));
            }
            else if (ActiveTrack != null && GetActiveTrackID() == trackDisplayID && SelectedTrack.IsPlaying) //TODO: see if third condition is necessary or if it is just redundant
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
                        string activeTrackID = GetActiveTrackID(); 
                        PlayCommand playCommand = request as PlayCommand;
                        if (ActiveTrack != null && activeTrackID != playCommand.TrackDisplayID)
                        {
                            Stop(activeTrackID);
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
                        Pause(undoPlayCommand.TrackDisplayID);
                    },
                    "PauseCommand" => () =>
                    {
                        PauseCommand undoPauseCommand = request as PauseCommand;
                        Play(undoPauseCommand.TrackDisplayID);
                    },
                    "NextItemCommand" => PreviousItem,
                    "PreviousItemCommand" => NextItem,
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
                Debug.Log("There is no active track.");
            }
        }

        public void Play(string trackDisplayID)
        {
            ObserverManager.ObserverManager.Instance.NotifyObservers("OnActiveTrackChanged", null);
            SetActiveTrack(TrackLibrary.Instance.GetTrackAtID(trackDisplayID));
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
                "OnActiveTrackIsDone" => NextItem,
                "OnSingleTrackSelected" => () =>
                {
                    SelectedTrack = TrackLibrary.Instance.GetTrackAtID((string)data);
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }
    }
}
