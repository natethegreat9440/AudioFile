using AudioFile.Model;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using AudioFile.Controller;


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

    [Serializable]
    public class TrackProperties
    {
        public string ConnectionString => SetupController.Instance.ConnectionString;

        private readonly HashSet<string> validProperties = new HashSet<string>()
        {
            "Title", "Artist", "Album", "Duration", "BPM", "Path", "TrackID", "AlbumTrackNumber"
        };
        public string GetProperty(string trackID, string property)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    if (!validProperties.Contains(property)) //Security check to prevent SQL injection attacks
                    {
                        throw new ArgumentException("Invalid property name");
                    }

                    command.CommandText = $"SELECT {property} FROM Tracks WHERE TrackID = @TrackID;";
                    command.Parameters.AddWithValue("@TrackID", trackID); //Security measure to prevent SQL injection attacks
                    return command.ExecuteScalar()?.ToString();
                }
            }
        }

        public Dictionary<string, string> GetAllProperties(string trackID)
        {
            var properties = new Dictionary<string, string>();

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Tracks WHERE TrackID = @TrackID;";
                    command.Parameters.AddWithValue("@TrackID", trackID);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            foreach (var property in validProperties)
                            {
                                properties[property] = reader[property]?.ToString();
                            }
                        }
                    }
                }
            }

            return properties;
        }

        public void SetProperty(string trackID, string property, string value)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    if (!validProperties.Contains(property)) //Security check to prevent SQL injection attacks
                    {
                        throw new ArgumentException("Invalid property name");
                    }

                    command.CommandText = $"UPDATE Tracks SET {property} = @Value WHERE TrackID = @TrackID;";
                    command.Parameters.AddWithValue("@Value", value); //Security measure to prevent SQL injection attacks
                    command.Parameters.AddWithValue("@TrackID", trackID); //Security measure to prevent SQL injection attacks
                    command.ExecuteNonQuery();
                }
            }
        }

        /*[SerializeField]
        Dictionary<string, string> trackProperties = new Dictionary<string, string>()
        {
            {"Title", "Untitled Track"},
            {"Artist", "Unknown Artist"},
            {"Album", "Unknown Album"},
            {"Duration", "--:--"},
            {"BPM", "--"},
            {"Path", "Unknown Path"},
            {"TrackID", "Error: No Track ID" },
            {"AlbumTrackNumber", "0" }
        };

        private readonly HashSet<string> cantRemoveProperties = new HashSet<string>()
        {
            "Title", "Artist", "Album", "Duration", "BPM", "Path", "TrackID", "AlbumTrackNumber"
        };

        public string GetProperty(string key)
        {
            if (trackProperties.ContainsKey(key))
            {
                return trackProperties[key];
            }
            else { Debug.Log($"Property: {key} does not exist."); }

            return null;
        }
        public Dictionary<string, string> GetAllProperties()
        {
            return new Dictionary<string, string>(trackProperties);
        }
        public void SetProperty(string key, string value)
        {
            if (trackProperties.ContainsKey(key))
            {
                trackProperties[key] = value;
            }
            else { Debug.Log($"Property: {key} does not exist."); }

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
        */

    }
}
