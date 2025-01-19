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
        public object GetProperty(int trackID, string property)
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
                    return command.ExecuteScalar();
                }
            }
        }

        public Dictionary<string, object> GetAllProperties(int trackID)
        {
            var properties = new Dictionary<string, object>();

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
                                var value = reader[property];
                                properties[property] = value != DBNull.Value ? value : null;
                            }
                        }
                    }
                }
            }

            return properties;
        }

        public void SetProperty(int trackID, string property, object value)
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
    }
}
