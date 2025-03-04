using AudioFile.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using AudioFile.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using Mono.Data.Sqlite;
using AudioFile.Controller;

namespace AudioFile.Model
{
    /// <summary>
    /// Concrete class for Track objects
    /// <remarks>
    /// Once part of a Composite design pattern implementation. Track was a leaf node. Tracks were the children to the TrackLibrary composite node, but now standalone objects outside of a composite design pattern.
    /// Track Properties (i.e., Title, Artist, etc.) are stored in a TrackProperties object for better cohesion. Uses a static factory method to create and initialize Tracks.
    /// Members: TrackID, IsPlaying, IsPaused, TrackProperties, _audioSource, _trackDuration, CreateTrack(), IsDone(), FormatTime(), GetTime(). 
    /// Inherits Play(), Pause(), Stop(), GetDuration(), SetTime() from MediaLibraryComponent and largely delegates to the playback methods of Unity AudioSource objects with minor additions to help facilitate program flow.
    /// Has _audioPlayableOutput and _audioPlayable properties that hold Unity AudioPlayableOutput and AudioClipPlayable objects for more advanced playback capabilities (so far I haven't really needed to use them, so could be candidates for removal later)
    /// Has default ToString() override implementations. Implements Initialize(), Update(), OnDestroy(), from MonoBehaviour.
    /// </remarks>
    /// <see cref="TrackProperties"/>
    /// <seealso cref="TrackLibraryController"/>
    /// <seealso cref="MediaLibraryComponent"/>
    /// <seealso cref="IPlayable"/>
    /// </summary>

    public class Track : MediaLibraryComponent, IPlayable
    {
        AudioSource _audioSource;
        AudioPlayableOutput _audioPlayableOutput;
        AudioClipPlayable _audioPlayable;

        [SerializeField]
        public TrackProperties TrackProperties;

        private string _trackDuration;

        bool _isPlaying = false;
        public bool IsPlaying { get { return _isPlaying; } private set { _isPlaying = value; } }

        bool _isPaused = false;
        public bool IsPaused { get { return _isPaused; } private set { _isPaused = value; } }
        public string ConnectionString => SetupController.Instance.ConnectionString;

        public int TrackID { get; set; }

        #region Track Setup
        // Static factory method to create and initialize Track
        public static Track CreateTrack(AudioClip loadedClip, List<string> metadata, bool isNewTrack = false, bool isUntagged = false)
        {
            string geniusUrl = string.Empty;

            string trackTitle = metadata[0];
            string contributingArtists = metadata[1];
            string trackAlbum = metadata[2];
            int albumTrackNumber = (metadata[3]) != null ? int.Parse(metadata[3]) : 0;
            if (metadata.Count > 5 && metadata[4] != null)
                geniusUrl = metadata[4];
            string loadedPath = metadata[^1]; //Returns whatever the last index element is, which should always be the path because it gets added after ExtractMetadata

            Track track = null;

            using (var connection = new SqliteConnection(SetupController.Instance.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    if (isNewTrack)
                    {
                        command.CommandText = @"
                        INSERT INTO Tracks (Title, Artist, Album, Duration, Path, AlbumTrackNumber, GeniusUrl)
                        VALUES (@Title, @Artist, @Album, @Duration, @Path, @AlbumTrackNumber, @GeniusUrl);
                        SELECT TrackID FROM Tracks WHERE rowid = last_insert_rowid();";
                    }
                    else if (isNewTrack == false && isUntagged == false)
                    {
                        command.CommandText = @"
                        UPDATE Tracks
                        SET Title = @Title, Artist = @Artist, Album = @Album, Duration = @Duration, Path = @Path, AlbumTrackNumber = @AlbumTrackNumber
                        WHERE Path = @Path;
                        SELECT TrackID FROM Tracks WHERE Path = @Path;";
                    }
                    else
                    {
                        command.CommandText = @"
                        SELECT TrackID FROM Tracks WHERE Path = @Path;";
                    }

                    command.Parameters.AddWithValue("@Title", trackTitle);
                    command.Parameters.AddWithValue("@Artist", contributingArtists);
                    command.Parameters.AddWithValue("@Album", trackAlbum);
                    command.Parameters.AddWithValue("@Duration", ""); // Initialize() will add this
                    command.Parameters.AddWithValue("@Path", loadedPath);
                    command.Parameters.AddWithValue("@AlbumTrackNumber", albumTrackNumber);
                    command.Parameters.AddWithValue("@GeniusUrl", geniusUrl); //This doesn't get reassigned when loading existing tracks

                    var result = command.ExecuteScalar();
                    int trackID = Convert.ToInt32(result);

                    // Create the track object
                    GameObject trackObject = new GameObject("Track_" + trackTitle);
                    track = trackObject.AddComponent<Track>();
                    track.Initialize(trackID, loadedClip, isNewTrack);
                    return track;
                }
            }
        }

        // Initialize method to set up TrackProperties reference, AudioSource, AudioPlayableOutput, track duration, track ID, and notify observers that a new track has been added if it is new to the library
        private void Initialize(int trackID, AudioClip loadedClip, bool isNewTrack)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = loadedClip;
            TrackProperties = new TrackProperties();

            _playableGraph = PlayableGraph.Create();
            _audioPlayableOutput = AudioPlayableOutput.Create(_playableGraph, "Audio", _audioSource);
            _audioPlayable = AudioClipPlayable.Create(_playableGraph, _audioSource.clip, false);
            _audioPlayableOutput.SetSourcePlayable(_audioPlayable);
            _playableHandle = _audioPlayable.GetHandle();

            _trackDuration = FormatTime(GetDuration());
            TrackProperties.SetProperty(trackID, "Duration", _trackDuration);

            TrackID = trackID;
            if (isNewTrack)
            {
                ObserverManager.Instance.NotifyObservers("OnNewTrackAdded", this);
            }
        }

        void Update()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                double currentTime = _audioSource.time;
                double clipLength = _audioSource.clip.length;
                float progress = (float)(currentTime / clipLength);
                ObserverManager.Instance.NotifyObservers("OnTrackFrameUpdate", progress);
            }
        }
        void OnDestroy()
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }

            if (_audioSource != null)
            {
                Destroy(_audioSource);
            }
        }
        public override string ToString()
        {
            return $"{TrackProperties.GetProperty(TrackID, "Title")} - {TrackProperties.GetProperty(TrackID, "Artist")}";
        }
        #endregion
        #region Playback method implementations

        //TODO: Make this a Utilities method??
        public string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            return string.Format("{0:0}:{1:00}", minutes, secs);
        }

        public override void Play(string trackDisplayID = "")
        {
            //_playableGraph.Play(); Doesn't affect playback. This kind of object just offers more advanced 
            // functionality than AudioSource does, but for now it might be overkill
            Debug.Log($"{this}_audioSource.time: {FormatTime((float)_audioSource.time)}");

            if (_audioSource.time > 0)
            {
                _audioSource.UnPause();
                Debug.Log($"Track {this} has been resumed at {FormatTime((float)_audioSource.time)}");
            }
            else
            {
                _audioSource.Play();
                Debug.Log($"Track {this} has been played");
            }
            IsPlaying = true;
            IsPaused = false;
        }

        public override void Pause(string trackDisplayID="")
        {
            //Pausing does not affect which track is known as the active track by the PlaybackController
            //_playableGraph.Stop();
            _audioSource.Pause();
            Debug.Log($"Track {this} has been paused at {FormatTime((float)_audioSource.time)}");
            IsPaused = true;
            IsPlaying = false;
        }
        public override void Stop(string trackDisplayID = "")
        {
            //Stopping a PlayableGraph essentially just pauses it. Need to manually set to zero
            //using AudioPlayable.SetTime(0.0) if using PlayableGraph
            //_playableGraph.Stop();
            _audioSource.Stop();
            Debug.Log($"Track {this} has been stopped. Time is now {FormatTime((float)_audioSource.time)}");
            IsPlaying = false;
            IsPaused = false;
            //Debug.Log($"AudioPlayable time: { FormatTime((float)_audioPlayable.GetTime())}");
            //Debug.Log($"AudioSource time: {FormatTime((float)_audioSource.time)}");

            ObserverManager.Instance.NotifyObservers("OnTrackStopped");

        }

        public override float GetDuration()
        {
            return (float)_audioSource.clip.length;
        }

        public override void SetTime(float time)
        {
            //_audioPlayable.SetTime(time);
            _audioSource.time = (float)time;
            Debug.Log($"Track {this} has been set to {FormatTime(_audioSource.time)}");
            //_audioSource.Play();
            IsPlaying = true;
            IsPaused = false;
        }

        public float GetTime()
        {
            return (float)_audioSource.time;
        }

        // Check if the track is done
        public bool IsDone()
        {
            if (_audioSource.time >= _audioSource.clip.length)
            {
                Debug.Log($"{this} is done");
                return !_audioSource.isPlaying;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}


