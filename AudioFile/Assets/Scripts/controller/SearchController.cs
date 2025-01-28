using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioFile.Model;
using AudioFile.Utilities;
using AudioFile.View;
using UnityEngine;
using Mono.Data.Sqlite;


namespace AudioFile.Controller
{
    public enum SearchProperties {  Title, Artist, Album, };
    public class SearchController : MonoBehaviour, IController
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<SearchController> _instance = new Lazy<SearchController>(CreateSingleton);

        // Private constructor to prevent instantiation
        private SearchController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static SearchController Instance => _instance.Value;

        private static SearchController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(SearchController).Name);

            // Add the SortController component to the GameObject
            return singletonObject.AddComponent<SearchController>();
        }

        public string ConnectionString => SetupController.Instance.ConnectionString;

        public List<int> SearchResults { get; private set; } = new List<int>();
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        private List<int> Search(string userQuery, SearchProperties searchProperty, string tableName = "Tracks")
        {
            List<int> results = new List<int>();
            string query = $"SELECT TrackID FROM {tableName} WHERE {searchProperty} LIKE @userQuery";

            using (SqliteConnection connection = new SqliteConnection(ConnectionString))
            {
                SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@userQuery", "%" + userQuery + "%");

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(reader.GetInt32(0));
                    }
                }
            }

            return results;
        }
        public void HandleRequest(object request, bool isUndo)
        {
            string command = request.GetType().Name;

            if (isUndo == false)
            {
                Action action = command switch
                {
                    "SearchCommand" => () =>
                    {
                        SearchCommand searchCommand = request as SearchCommand;
                        foreach (var property in Enum.GetValues(typeof(SearchProperties)).Cast<SearchProperties>())
                        {
                            SearchResults.AddRange(Search(searchCommand.UserQuery, property, searchCommand.QueryTable));
                        }

                        if (SearchResults.Count > 0)
                        {
                            ObserverManager.Instance.NotifyObservers("SearchResultsFound", SearchResults);
                        }
                        else
                        {
                            ObserverManager.Instance.NotifyObservers("AudioFileError", "Search results not found.");
                        }
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
                    "SearchCommand" => () =>
                    {
                        //TODO: Delegate to UITrackDisplayManager to undo the search/reset how UITrackDisplayManager is displaying
                    },
                    //Add more switch arms here as needed
                    _ => () => Debug.LogWarning($"Unhandled command: {request}")
                };
                action();
            }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
