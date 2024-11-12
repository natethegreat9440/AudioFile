using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioFile.Model;
using AudioFile.Controller;
using AudioFile.ObserverManager;
using UnityEngine;
using UnityEngine.UI;

namespace AudioFile.View
{
    /// <summary>
    /// Concrete class for managing/updating the UI Playback Bar.
    /// <remarks>
    /// Playback bar is a Unity Slide object that updates it's position relative to the current time of the track Members:
    /// Implements Start() from MonoBehaviour. Implements AudioFileUpdate() from IAudioFileObserver. 
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IAudioFileObserver"/>
    /// </summary>
    public class UIPlaybackBarManager : MonoBehaviour, IAudioFileObserver //IAudioFileObserver required method AudioFileUpdate(string observationType, object data) is last method in class
    {
        public Slider slider;

        public void Start()
        {
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackFrameUpdate", this);
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackFrameUpdate", this);
        }
        //TODO: Create an onClick method for anywhere in the slider area that will seek the time of the track based on where the user clicks
        //This class should create a Seek command and if the current track is playing (Playback controller will check this) then the Seek command will 
        //take the current time based on playback bar position (current time stored for a potential undo command) and take the "seeked time" based on where the user clicked 
        //have the PlaybackController HandleRequest(new SeekCommand(currentTime, seekedTime))
        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnTrackFrameUpdate" => () => slider.value = (float)data,
                "OnTrackStopped" => () => slider.value = 0,
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }
    }
}
