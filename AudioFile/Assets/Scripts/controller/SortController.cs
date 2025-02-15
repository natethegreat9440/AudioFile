using System.Collections.Generic;
using AudioFile.View;
using AudioFile.Utilities;
using System;
using UnityEngine;
using System.Linq;
using Mono.Data.Sqlite;


namespace AudioFile.Controller
{
    /// <summary>
    /// Singleton Sort Controller in AudioFile used for controlling sort behavior as triggered by the UI, loading UITrackListDisplayManager, and PlaybackController among others using SQLite injections. 
    /// This class file also contains the LibrarySortProperties enum which is used internally to specify the default sort method for the library, which is the default model view upon load.
    /// <remarks>
    /// Members: SortForward(), SortReverse(), RestoreDefaultOrder(), RefreshSorting(). Coroutine LoadAudioClipFromFile(). Implements HandleRequest() from IController. Implements AudioFileUpdate() from IAudioFileObserver.
    /// This controller has no implementation for IController methods Initialize() or Dispose() (yet).
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IController"/>
    /// <seealso cref="IAudioFileObserver"/>
    /// </summary>
    public enum LibrarySortProperties { AlbumTrackNumber, Title, Album, Artist, } //AlbumTrackNumber intentionally at the front here
    public class SortController : MonoBehaviour, IController, IAudioFileObserver
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<SortController> _instance = new Lazy<SortController>(CreateSingleton);

        // Private constructor to prevent instantiation
        private SortController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static SortController Instance => _instance.Value;

        private static SortController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(SortController).Name);

            // Add the SortController component to the GameObject
            return singletonObject.AddComponent<SortController>();
        }

        public string ConnectionString => SetupController.Instance.ConnectionString;

        public List<string> CurrentSortOrdering { get; private set; } //CurrentSQLQueryInject refers to this. Don't remove. May not need as a public property however, but we'll keep it this way for now

        public string CurrentSQLQueryInject { get; private set; }

        bool isFiltered => SearchController.Instance.IsFiltered;

        List<int> searchResults => SearchController.Instance.SearchResults;

        string FilterSQLQueryInject
        {
            get
            {
                if (isFiltered)
                    return $"WHERE TrackID IN({ string.Join(",", searchResults)})";
                else
                    return "";
            }
        }
        public void Start()
        {
            ObserverManager.Instance.RegisterObserver("OnNewTrackAdded", this);
            ObserverManager.Instance.RegisterObserver("SearchCleared", this);
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnNewTrackAdded" => () =>
                {
                    RefreshSorting();
                },
                "SearchCleared" => () =>
                {
                    RefreshSorting();
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }
        public void HandleRequest(object request, bool isUndo)
        {
            //Add methods to log these commands with the UndoController
            SortCommand command = request as SortCommand;

            if (isUndo == false)
            {
                Action action = command.State switch
                {
                    SortButtonState.Forward => () => 
                    {
                        SortForward(command.CollectionToSort, command.SortBy);
                    },
                    SortButtonState.Reverse => () => 
                    {
                        SortReverse(command.CollectionToSort, command.SortBy);
                    },
                    SortButtonState.Default => () =>
                    {
                        RestoreDefaultOrder(command.CollectionToSort);
                    },
                    //Add more switch arms here as needed
                    _ => () => Debug.LogWarning($"Unhandled command: {request}")
                };

                action();
            }
            //Undo handling
            else
            {
                Action action = command.State switch
                {
                    SortButtonState.Forward => () =>
                    {
                        RestoreDefaultOrder(command.CollectionToSort);
                    },
                    SortButtonState.Reverse => () =>
                    {
                        SortForward(command.CollectionToSort, command.SortBy);
                    },
                    SortButtonState.Default => () =>
                    {
                        SortReverse(command.CollectionToSort, command.SortBy);
                    },
                    //Add more switch arms here as needed
                    _ => () => Debug.LogWarning($"Unhandled undo command: {request}")
                };

                action();
            }
        }
        private void SortForward(string collectionToSort, string mainSortProperty)
        {
            //Note that if AlbumTrackNumber is passed to this ever as the mainSortProperty it will cause some issues. However, that's not likely to happen with the current design
            Debug.Log($"Sorting {collectionToSort} in forward order by {mainSortProperty}");
            var sortedTrackIDs = new List<int>();

            if (collectionToSort == "Tracks")
            {
                //Note that LibrarySortProperties { AlbumTrackNumber, Title, Album, Artist } and the odd order of CurrentSortOrdering is intentional/correct
                //TODO: There may be a cleaner way to do it, but this was all I could think of when I initially wrote this pre-SQLite refactoring

                var sortProperties = Enum.GetValues(typeof(LibrarySortProperties))
                    .Cast<LibrarySortProperties>()
                    .Select(e => e.ToString())
                    .Where(e => e != mainSortProperty)
                    .ToList();

                CurrentSortOrdering = new List<string> { mainSortProperty, sortProperties[2], sortProperties[0], sortProperties[1] };

                HandleSortForwardSQLCommand(sortedTrackIDs);

                if (sortedTrackIDs.Count > 0)
                    ObserverManager.Instance.NotifyObservers("OnCollectionReordered", sortedTrackIDs);
            }
        }

        private void SortReverse(string collectionToSort, string mainSortProperty)
        {
            //Note that if AlbumTrackNumber is passed to this ever as the mainSortProperty it will cause some issues. However, that's not likely to happen with the current interface
            Debug.Log($"Sorting {collectionToSort} in reverse order by {mainSortProperty}");
            var sortedTrackIDs = new List<int>();

            if (collectionToSort == "Tracks")
            {
                //Note that LibrarySortProperties { AlbumTrackNumber, Title, Album, Artist } and the odd order of CurrentSortOrdering is intentional/correct
                //TODO: There may be a cleaner way to do it, but this was all I could think of when I initially wrote this pre-SQLite refactoring

                var sortProperties = Enum.GetValues(typeof(LibrarySortProperties))
                    .Cast<LibrarySortProperties>()
                    .Select(e => e.ToString())
                    .Where(e => e != mainSortProperty)
                    .ToList();

                CurrentSortOrdering = new List<string> { mainSortProperty, sortProperties[2], sortProperties[0], sortProperties[1] };

                HandleSortReverseSQLCommand(sortedTrackIDs);

                if (sortedTrackIDs.Count > 0)
                    ObserverManager.Instance.NotifyObservers("OnCollectionReordered", sortedTrackIDs);
            }
        }

        public void RefreshSorting()
        {
            SortButton buttonToSortBy = null;

            foreach (var button in UISortButtonsManager.Instance.SortButtons)
            {
                buttonToSortBy = GetButtonToSortBy(buttonToSortBy, button);
            }

            if (buttonToSortBy == null)
            {
                RestoreDefaultOrder(UITrackListDisplayManager.Instance.TracksDisplayed);
            }
            else //This block ensures the sort state persists as new tracks get added
            {
                DetermineSortDirection(buttonToSortBy);
            }
        }

        public void RestoreDefaultOrder(string collectionToSort)
        {
            Debug.Log($"Setting {collectionToSort} in default order");
            var sortedTrackIDs = new List<int>();

            if (collectionToSort == "Tracks")
            {
                HandleDefaultSortSQLCommand(sortedTrackIDs);

                if (sortedTrackIDs.Count > 0)
                    ObserverManager.Instance.NotifyObservers("OnCollectionReordered", sortedTrackIDs);
            }
        }

        private void HandleSortForwardSQLCommand(List<int> sortedTrackIDs)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                //ASC = ascending
                //Adds a filter if a search is active with {FilterSQLQueryInject}. If search is not active, "" is passed as the FilterSQLQueryInject
                string query = $@"
                        SELECT * FROM Tracks
                        {FilterSQLQueryInject}
                        ORDER BY 
                            {CurrentSortOrdering[0]} ASC,
                            {CurrentSortOrdering[1]} ASC,
                            {CurrentSortOrdering[2]} ASC,
                            {CurrentSortOrdering[3]} ASC";

                CurrentSQLQueryInject = query;

                using (var command = new SqliteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var result = reader["TrackID"];
                            var trackID = Convert.ToInt32(result);

                            sortedTrackIDs.Add(trackID);
                        }
                    }
                }
            }
        }

        private void HandleSortReverseSQLCommand(List<int> sortedTrackIDs)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                //ASC = ascending
                //DESC = descending (not Describe as this is not MySQL)
                //Adds a filter if a search is active with {FilterSQLQueryInject}. If search is not active, "" is passed as the FilterSQLQueryInject
                string query = $@"
                        SELECT * FROM Tracks
                        {FilterSQLQueryInject}
                        ORDER BY 
                            {CurrentSortOrdering[0]} DESC,
                            {CurrentSortOrdering[1]} DESC,
                            {CurrentSortOrdering[2]} ASC,
                            {CurrentSortOrdering[3]} ASC";

                CurrentSQLQueryInject = query;

                using (var command = new SqliteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var result = reader["TrackID"];
                            int trackID = Convert.ToInt32(result);

                            sortedTrackIDs.Add(trackID);
                        }
                    }
                }
            }
        }

        private void HandleDefaultSortSQLCommand(List<int> sortedTrackIDs)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                //ASC = ascending
                string query = $@"
                        SELECT * FROM Tracks
                        {FilterSQLQueryInject}
                        ORDER BY 
                            Artist ASC,
                            Album ASC,
                            AlbumTrackNumber ASC,
                            Title ASC";

                CurrentSQLQueryInject = query;

                using (var command = new SqliteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["TrackID"] != DBNull.Value)
                            {
                                var trackID = Convert.ToInt32(reader["TrackID"]);
                                sortedTrackIDs.Add(trackID);
                            }
                            else
                            {
                                Debug.LogWarning("TrackID is null");
                            }
                        }
                    }
                }
            }
        }

        private void DetermineSortDirection(SortButton buttonToSortBy)
        {
            if (buttonToSortBy.State == SortButtonState.Forward)
            {
                SortForward(UITrackListDisplayManager.Instance.TracksDisplayed, buttonToSortBy.SortProperty);
            }
            else if (buttonToSortBy.State == SortButtonState.Reverse)
            {
                SortReverse(UITrackListDisplayManager.Instance.TracksDisplayed, buttonToSortBy.SortProperty);
            }
        }

        private static SortButton GetButtonToSortBy(SortButton buttonToSortBy, SortButton button)
        {
            if (button.State != SortButtonState.Default)
            {
                buttonToSortBy = button;
            }

            return buttonToSortBy;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
