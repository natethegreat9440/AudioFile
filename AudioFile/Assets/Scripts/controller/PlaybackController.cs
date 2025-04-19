using System.Collections.Generic;
using AudioFile.Model;
using AudioFile.View;
using AudioFile.Utilities;
using System;
using UnityEngine;

namespace AudioFile.Controller
{
    /// <summary>
    /// Singleton Playback Controller in AudioFile used for controlling playback functions of tracks within TrackLibrary and Playlists
    /// <remarks>
    /// May be modified later to control synchronization of track and visualizer playback. 
    /// Will be modified to pass along all commands to the CommandStackController for undo/redo operations.
    /// Members: ActiveTrack, SelectedTrack, GetCurentTrackIndex(), HandlePlayPauseButton(), SetActiveTrack(), SetSelectedTrack(),
    /// Play(), Pause(), Stop(), NextItem(), PreviousItem(), Skip(), Seek(), GetTime().
    /// Implements Awake(), Start(), and Update() from MonoBehaviour. Implements AudioFileUpdate() from IAudioFileObserver. Implements HandleUserRequest() from IController.
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

        List<int> SearchResults => SearchController.Instance.SearchResults;

        private List<Track> TrackList => TrackLibraryController.Instance.TrackList;

        bool isFiltered => SearchController.Instance.IsFiltered;

        private string CurrentSQLQueryInject => SortController.Instance.CurrentSQLQueryInject;

        private bool IsShuffleMode = false;

        private List<int> PlayedTracks = new List<int>();

        public int ActiveTrackIndex
        {
            get
            {
                if (isFiltered && SearchResults.Contains(ActiveTrack.TrackID))
                    return SearchController.Instance.GetSearchResultsIndex(ActiveTrack.TrackID) + 1;//Need to add 1 because SearchResults is 0-indexed and TrackLibraryController.Instance.GetTrackIndex is 1-indexed since it access a SQLite DB directly
                else
                    return TrackLibraryController.Instance.GetTrackIndex(ActiveTrack.TrackID);
            }
        }
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
            ObserverManager.Instance.RegisterObserver("OnSingleTrackSelected", this);
            ObserverManager.Instance.RegisterObserver("OnActiveTrackIsDone", this);
        }

        void Update()
        {
            try
            {
                if (ActiveTrack != null && ActiveTrack.IsDone())
                {
                    Debug.Log("Track has finished playing.");
                    ObserverManager.Instance.NotifyObservers("OnActiveTrackIsDone", null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Auto-play failed: {e}");
            }
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnActiveTrackIsDone" => () => { NextItem(); },
                "OnSingleTrackSelected" => () =>
                {
                    SetSelectedTrack(TrackLibraryController.Instance.GetTrackAtID((int)data));
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }

        public void HandleUserRequest(object request, bool isUndo = false)
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

        // Method to toggle shuffle mode
        public void ToggleShuffleMode()
        {
            IsShuffleMode = !IsShuffleMode;
            PlayedTracks.Clear(); // Reset played tracks when toggling shuffle mode
            Debug.Log($"Shuffle mode is now {(IsShuffleMode ? "enabled" : "disabled")}");
        }

        // Update NextItem method to support shuffle mode
        public bool NextItem()
        {
            if (IsShuffleMode)
            {
                if (PlayedTracks.Count >= TrackList.Count)
                {
                    Debug.Log("All tracks have been played in shuffle mode. Resetting.");
                    PlayedTracks.Clear();
                }

                int randomIndex;
                do
                {
                    randomIndex = UnityEngine.Random.Range(0, TrackList.Count);
                } while (PlayedTracks.Contains(randomIndex));

                PlayedTracks.Add(randomIndex);
                HandlePlayingNextTrack(randomIndex + 1); // Adjust for 1-based indexing
                return true;
            }

            // Existing logic for non-shuffle mode
            int nextTrackIndex, tracksLength;
            SetNextTrackIndexAndGetTracksLength(out nextTrackIndex, out tracksLength);

            if (nextTrackIndex < tracksLength + 1 && nextTrackIndex > 0)
            {
                HandlePlayingNextTrack(nextTrackIndex);
                return true;
            }
            else
            {
                Debug.Log("Reached the end of the playlist.");
                ObserverManager.Instance.NotifyObservers("OnTrackListEnd", ActiveTrackIndex);
                return false;
            }
        }

        public void PreviousItem()
        {
            int prevTrackIndex = SetPrevTrackIndex();

            if (prevTrackIndex > 0)
            {
                HandlePlayingPrevTrack(prevTrackIndex);
            }
            else
            {
                Debug.Log("Reached the front of the playlist.");
                ObserverManager.Instance.NotifyObservers("OnTrackListEnd", ActiveTrackIndex);
            }
        }

        public void Skip()
        {
            if (ActiveTrack != null && !NextItem())
            {
                Debug.Log("Delegating to PreviousItem() because the end of the playlist was reached.");
                PreviousItem(); // Fallback to previous item
            }
            ObserverManager.Instance.NotifyObservers("OnTrackSkipped", ActiveTrack); //Passes the track name if possible when skipped
        }

        public void Seek(float newTime)
        {
            ActiveTrack.SetTime(newTime);
        }

        public float GetTime()
        {
            return ActiveTrack.GetTime();
        }

        public void SetActiveTrack(Track track)
        {
            if (track != null)
            {
                ActiveTrack = track;
                Debug.Log($"Active track set to: {ActiveTrack} with ID: {ActiveTrack.TrackID}");
                ObserverManager.Instance.NotifyObservers("OnActiveTrackChanged", null);
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

                ObserverManager.Instance.NotifyObservers("OnSelectedTrackSetComplete", SelectedTrack.TrackID);
            }
            else
            {
                Debug.Log("There is no selected track.");
            }
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
                HandleUserRequest(new PlayCommand(trackDisplayID));
            }
            else if (ActiveTrack != null && ActiveTrack.TrackID == trackDisplayID && SelectedTrack.IsPlaying) //TODO: see if third condition is necessary or if it is just redundant
            {
                // Pause the active track
                HandleUserRequest(new PauseCommand(trackDisplayID));
            }
            else
            {
                // Play the active/selected track
                HandleUserRequest(new PlayCommand(trackDisplayID));
            }
        }

        private void HandlePlayingNextTrack(int nextTrackIndex)
        {
            Stop();

            int nextTrackID;
            Track nextTrack;
            SetNextTrack(nextTrackIndex, out nextTrackID, out nextTrack);

            SetActiveTrack(nextTrack);

            ObserverManager.Instance.NotifyObservers("OnActiveTrackCycled", ActiveTrackIndex);
            ObserverManager.Instance.NotifyObservers("OnActiveTrackChanged", null);

            Play(nextTrackID);
        }

        private void HandlePlayingPrevTrack(int prevTrackIndex)
        {
            Stop();
            int prevTrackID;
            Track prevTrack;
            SetPrevTrack(prevTrackIndex, out prevTrackID, out prevTrack);

            SetActiveTrack(prevTrack);

            ObserverManager.Instance.NotifyObservers("OnActiveTrackCycled", ActiveTrackIndex);
            ObserverManager.Instance.NotifyObservers("OnActiveTrackChanged", null);

            Play(prevTrackID);
        }

        private void SetNextTrack(int nextTrackIndex, out int nextTrackID, out Track nextTrack)
        {
            nextTrackID = -1;
            nextTrack = null;
            if (isFiltered && SearchResults.Contains(ActiveTrack.TrackID))
            {
                nextTrackID = SearchController.Instance.SearchResults[nextTrackIndex - 1]; //-1 to convert from 1-indexed to 0-indexed since SearchResults is 0-indexed
            }
            else
            {
                nextTrackID = TrackLibraryController.Instance.GetTrackIDAtIndex(nextTrackIndex);
            }

            nextTrack = TrackLibraryController.Instance.GetTrackAtID(nextTrackID);
        }

        private void SetPrevTrack(int prevTrackIndex, out int prevTrackID, out Track prevTrack)
        {
            prevTrackID = -1;
            prevTrack = null;
            if (isFiltered && SearchResults.Contains(ActiveTrack.TrackID))
            {
                prevTrackID = SearchController.Instance.SearchResults[prevTrackIndex - 1]; //-1 to convert from 1-indexed to 0-indexed since SearchResults is 0-indexed
            }
            else
            {
                prevTrackID = TrackLibraryController.Instance.GetTrackIDAtIndex(prevTrackIndex);
            }

            prevTrack = TrackLibraryController.Instance.GetTrackAtID(prevTrackID);
        }

        private void SetNextTrackIndexAndGetTracksLength(out int nextTrackIndex, out int tracksLength)
        {
            nextTrackIndex = -1;
            tracksLength = -2;
            if (isFiltered && SearchResults.Contains(ActiveTrack.TrackID)) //TODO: The isFiltered = true conditions in this method need more work
            {
                //Adding 2 to the index here because the TrackLibraryController.Instance.GetTrackIndex queries a SQLite table that is non-zero indexed and starts at 1 (+1 for converting from 0-indexed to 1-indexed and another +1 to get the next index)
                //Conversion necessary so they both approach the next if statement on similar basis. +1 +1 shown for verbosity here
                nextTrackIndex = SearchController.Instance.SearchResults.IndexOf(ActiveTrack.TrackID) + 1 + 1;
                tracksLength = SearchController.Instance.SearchResults.Count;
            }
            else
            {
                //Note that the GetTrackIndex queries a SQLite table that is non-zero indexed and starts at 1
                nextTrackIndex = TrackLibraryController.Instance.GetNextTrackIndex(ActiveTrack.TrackID); 
                tracksLength = TrackLibraryController.Instance.GetTracksLength();
            }
        }

        private int SetPrevTrackIndex()
        {
            int prevTrackIndex = -1;

            if (isFiltered && SearchResults.Contains(ActiveTrack.TrackID))
            {
                //Effectively not changing the index here because the TrackLibraryController.Instance.GetTrackIndex queries a SQLite table that is non-zero indexed and starts at 1 (+1 for converting from 0-indexed to 1-indexed and another -1 to get the previous index)
                //Conversion necessary so they both approach the next if statement on similar basis. +1 -1 shown for verbosity here
                prevTrackIndex = SearchController.Instance.SearchResults.IndexOf(ActiveTrack.TrackID) + 1 - 1;
            }
            else
            {
                //Note that the GetTrackIndex queries a SQLite table that is non-zero indexed and starts at 1
                prevTrackIndex = TrackLibraryController.Instance.GetPrevTrackIndex(ActiveTrack.TrackID); 
            }

            return prevTrackIndex;
        }

        public void HandleActiveTrackAfterTrackRemoval(List<int> trackDisplayIDs)
        {
            if (trackDisplayIDs.Count >= UITrackListDisplayManager.Instance.AllTrackDisplayTransforms.Count)
            {
                ActiveTrack = null;
                SelectedTrack = null;
            }
            else
            {
                // Reorder trackDisplayIDs to match the order in UITrackListDisplayManager
                trackDisplayIDs = UITrackListDisplayManager.Instance.GetOrderedTrackDisplayIDs(trackDisplayIDs);

                // Get the last element in the tracks to be removed and then either try to go to the next or previous item after that
                SetActiveTrack(TrackLibraryController.Instance.GetTrackAtID(trackDisplayIDs[trackDisplayIDs.Count - 1]));

                if (!NextItem())
                {
                    SetActiveTrack(TrackLibraryController.Instance.GetTrackAtID(trackDisplayIDs[0]));
                    PreviousItem();
                }
            }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
