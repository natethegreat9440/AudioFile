using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioFile.Model;
using AudioFile.ObserverManager;
using AudioFile.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace AudioFile.View
{
    public class UIPlaybackBarManager : MonoBehaviour, IAudioFileObserver
    {
        public Slider slider;

        public void Start()
        {
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackFrameUpdate", this);
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackStopped", this);
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            if (observationType == "OnTrackFrameUpdate")
            {
                slider.value = (float)data;
            }
            else if (observationType == "OnTrackStopped")
            {
                slider.value = 0;
            }
        }

    }
}
