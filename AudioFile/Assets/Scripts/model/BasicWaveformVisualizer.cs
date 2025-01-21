//Starting point for a basic waveform visualizer. Obviously will have to encapsulate the rendering logic out from the Track class
//Track class can hold the _waveformSamples float array and NotifyObservers (i.e, the VisualizerDisplayManager in the View Layer)
//The VisualizerDisplayManager in the View Layer will then render the waveform samples that on it's Update method
//There is a WaveformRendering method at the bottom here I might be able to reuse

/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    using global::AudioFile.Model;
    using UnityEngine;
    using UnityEngine.Audio;
    using UnityEngine.Playables;

    namespace AudioFile.Model
    {
        public class Track : MediaLibraryComponent, IPlayable
        {
            AudioSource _audioSource;
            AudioPlayableOutput _audioPlayableOutput;
            AudioClipPlayable _audioPlayable;
            public TrackProperties TrackProperties;
            private string _trackDuration;

            private float[] _waveformSamples = new float[1024]; // Array to store waveform samples

            #region Setup/Unity methods
            public static Track CreateTrack(AudioClip loadedClip, string trackTitle = "Untitled Track",
                                            string trackArtist = "Unknown Artist", string trackAlbum = "Unknown Album", string name = "A track")
            {
                GameObject trackObject = new GameObject("Track_" + trackTitle);
                Track track = trackObject.AddComponent<Track>();
                track.Initialize(loadedClip, trackTitle, trackArtist, trackAlbum);
                return track;
            }

            private void Initialize(AudioClip loadedClip, string trackTitle, string trackArtist, string trackAlbum)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.clip = loadedClip;
                TrackProperties = new TrackProperties();
                TrackProperties.SetProperty("Title", trackTitle);
                TrackProperties.SetProperty("Artist", trackArtist);
                TrackProperties.SetProperty("Album", trackAlbum);

                _playableGraph = PlayableGraph.Create();
                _audioPlayableOutput = AudioPlayableOutput.Create(_playableGraph, "Audio", _audioSource);
                _audioPlayable = AudioClipPlayable.Create(_playableGraph, _audioSource.clip, false);
                _audioPlayableOutput.SetSourcePlayable(_audioPlayable);
                _playableHandle = _audioPlayable.GetHandle();

                _trackDuration = FormatTime(GetDuration());
                TrackProperties.SetProperty("Duration", _trackDuration);
            }

            void Update()
            {
                if (_playableGraph.IsValid() && _playableGraph.IsPlaying())
                {
                    double currentTime = _audioPlayable.GetTime();
                    double clipLength = _audioPlayable.GetDuration();
                    float progress = (float)(currentTime / clipLength);
                    AudioFile.ObserverManager.Instance.NotifyObservers("OnTrackFrameUpdate", progress);

                    // Update waveform samples
                    _audioSource.GetOutputData(_waveformSamples, 0);
                    RenderWaveform();
                }
            }

            void OnDestroy()
            {
                if (_playableGraph.IsValid())
                {
                    _playableGraph.Destroy();
                }
            }

            public override string ToString()
            {
                return $"{TrackProperties.GetProperty("Title")} - {TrackProperties.GetProperty("Artist")}";
            }
            #endregion

            #region Playback method implementations
            string FormatTime(float seconds)
            {
                int minutes = Mathf.FloorToInt(seconds / 60);
                int secs = Mathf.FloorToInt(seconds % 60);
                return string.Format("{0:0}:{1:00}", minutes, secs);
            }

            public override void Play(int index = 0)
            {
                _playableGraph.Play();
                _audioSource.Play();
                Debug.Log($"Track {this} has been played");
            }

            public override void Pause(int index = 0)
            {
                _playableGraph.Stop();
                _audioSource.Pause();
                Debug.Log($"Track {this} has been paused");
            }

            public override void Stop(int index = 0)
            {
                _playableGraph.Stop();
                _audioSource.Stop();
                _audioPlayable.SetTime(0.0);
                Debug.Log($"Track {this} has been stopped");
                Debug.Log($"AudioPlayable time: {FormatTime((float)_audioPlayable.GetTime())}");
                Debug.Log($"AudioSource time: {FormatTime((float)_audioSource.time)}");

                AudioFile.ObserverManager.Instance.NotifyObservers("OnTrackStopped");
            }

            public override float GetDuration()
            {
                return (float)_audioPlayable.GetDuration();
            }

            public override void SetTime(float time)
            {
                _audioPlayable.SetTime(time);
                _audioSource.time = (float)time;
            }

            public bool IsDone()
            {
                if (_audioPlayable.IsValid())
                {
                    return _audioPlayable.IsDone();
                }
                else return false;
            }
            #endregion

            #region Waveform Rendering
            void RenderWaveform()
            {
                // Implement your waveform rendering logic here
                // For example, you could use a LineRenderer or draw the waveform in a UI element
                for (int i = 0; i < _waveformSamples.Length; i++)
                {
                    // Normalize the sample value
                    float sample = _waveformSamples[i] * 0.5f + 0.5f;
                    // Map the sample value to a visual representation
                    // This is just a placeholder for your actual rendering logic
                    Debug.DrawLine(new Vector3(i, 0, 0), new Vector3(i, sample, 0), Color.green);
                }
            }
            #endregion
        }
    }
}*/
