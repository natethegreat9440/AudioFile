using AudioFile.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace AudioFile.Model
{
    public class TrackProperties
    {
        Dictionary<string, string> trackProperties = new Dictionary<string, string>()
        {
            {"Title", "Untitled Track"},
            {"Artist", "Unknown Artist"},
            {"Album", "Unknown Album"},
            {"Duration", "--:--"},
            {"BPM", "--" }
        };

        private readonly HashSet<string> cantRemoveProperties = new HashSet<string>()
        {
            "Title", "Artist", "Album", "Duration", "BPM"
        };

        public string GetProperty(string key)
        {
            if (trackProperties.ContainsKey(key))
            {
                return trackProperties[key];
            }
            return null;
        }

        public void SetProperty(string key, string value)
        {
            if (trackProperties.ContainsKey(key))
            {
                trackProperties[key] = value;
            }
        }

        public void AddProperty(string key, string value)
        {
            if (!trackProperties.ContainsKey(key))
            {
                trackProperties.Add(key, value);
            }
        }

        public void RemoveProperty(string key)
        {
            if (!cantRemoveProperties.Contains(key) && trackProperties.ContainsKey(key))
            {
                trackProperties.Remove(key);
            }
        }
    }
}
