using AudioFile.Model;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using System.Collections.Generic;


namespace AudioFile.Model
{
    /// <summary>
    /// Concrete class for holding Track Properties, which are encapsulated away from Track objects themselves in a trackProperties Dictionary object to help with cohesion and management of Track metadata down the road
    /// <remarks>
    /// Members: trackProperties, cantRemoveProperties, GetProperty, SetProperty, AddProperty, RemoveProperty
    /// </remarks>
    /// <see cref="Track"/>
    /// <seealso cref="TrackLibrary"/>
    /// </summary>

    public class TrackProperties
    {
        Dictionary<string, string> trackProperties = new Dictionary<string, string>()
        {
            {"Title", "Untitled Track"},
            {"Artist", "Unknown Artist"},
            {"Album", "Unknown Album"},
            {"Duration", "--:--"},
            {"BPM", "--"},
            {"Path", "Unknown Path"}
        };

        private readonly HashSet<string> cantRemoveProperties = new HashSet<string>()
        {
            "Title", "Artist", "Album", "Duration", "BPM", "LoadedPath", "TrackID"
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
