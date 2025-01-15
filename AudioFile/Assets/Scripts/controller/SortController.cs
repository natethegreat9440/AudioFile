using System.Collections;
using System.Collections.Generic;
using AudioFile;
using System.Windows.Forms;
using AudioFile.Model;
using AudioFile.View;
using AudioFile.ObserverManager;
using System;
using UnityEngine;
using System.Windows.Forms.VisualStyles;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Collections;


namespace AudioFile.Controller
{
    public enum LibrarySortProperties { AlbumTrackNumber, Title, Album, Artist } //AlbumTrackNumber intentionally at the front here
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

            // Add the PlayBackController component to the GameObject
            return singletonObject.AddComponent<SortController>();
        }

        //TODO: Add reference variables for the Sorted State and Sorted Property??
        public void Initialize()
        {
            throw new NotImplementedException();
        }
        public void Start()
        {
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnNewTrackAdded", this);
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
            //Note that if AlbumTrackNumber is passed to this ever as the mainSortProperty it will cause some issues. However, that's not likely to happen with the current interface
            Debug.Log($"Sorting {collectionToSort} in forward order by {mainSortProperty}");
            var sortedTrackList = new List<Track>();

            if (collectionToSort == "library")
            {
                var sortProperties = Enum.GetValues(typeof(LibrarySortProperties))
                    .Cast<LibrarySortProperties>()
                    .Select(e => e.ToString())
                    .Where(e => e != mainSortProperty)
                    .ToList();

                sortedTrackList = TrackLibrary.Instance.TrackList
                    .OrderBy(track =>
                    {
                        var value = track.TrackProperties.GetProperty(mainSortProperty);
                        return mainSortProperty == "AlbumTrackNumber" && int.TryParse(value, out int parsedValue)
                            ? (object)parsedValue
                            : value;
                    })
                    .ThenBy(track =>
                    {
                        var value = track.TrackProperties.GetProperty(sortProperties[2]);
                        return sortProperties[2] == "AlbumTrackNumber" && int.TryParse(value, out int parsedValue)
                            ? (object)parsedValue
                            : value;
                    })
                    .ThenBy(track =>
                    {
                        var value = track.TrackProperties.GetProperty(sortProperties[0]);
                        return sortProperties[0] == "AlbumTrackNumber" && int.TryParse(value, out int parsedValue)
                            ? (object)parsedValue
                            : value;
                    })
                    .ThenBy(track =>
                    {
                        var value = track.TrackProperties.GetProperty(sortProperties[1]);
                        return sortProperties[1] == "AlbumTrackNumber" && int.TryParse(value, out int parsedValue)
                            ? (object)parsedValue
                            : value;
                    })
                    .ToList();

                    TrackLibrary.Instance.TrackList = sortedTrackList;
            }

            if (sortedTrackList.Count > 0)
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnCollectionReordered", sortedTrackList);
        }
        private void SortReverse(string collectionToSort, string mainSortProperty)
        {
            Debug.Log($"Sorting {collectionToSort} in forward order by {mainSortProperty}");
            var sortedTrackList = new List<Track>();

            if (collectionToSort == "library")
            {
                var sortProperties = Enum.GetValues(typeof(LibrarySortProperties))
                    .Cast<LibrarySortProperties>()
                    .Select(e => e.ToString())
                    .Where(e => e != mainSortProperty)
                    .ToList();

                sortedTrackList = TrackLibrary.Instance.TrackList
                    .OrderByDescending(track =>
                    {
                        var value = track.TrackProperties.GetProperty(mainSortProperty);
                        return mainSortProperty == "AlbumTrackNumber" && int.TryParse(value, out int parsedValue)
                            ? (object)parsedValue
                            : value;
                    })
                    .ThenByDescending(track =>
                    {
                        var value = track.TrackProperties.GetProperty(sortProperties[2]);
                        return sortProperties[2] == "AlbumTrackNumber" && int.TryParse(value, out int parsedValue)
                            ? (object)parsedValue
                            : value;
                    })
                    .ThenBy(track =>
                    {
                        var value = track.TrackProperties.GetProperty(sortProperties[0]);
                        return sortProperties[0] == "AlbumTrackNumber" && int.TryParse(value, out int parsedValue)
                            ? (object)parsedValue
                            : value;
                    })
                    .ThenBy(track =>
                    {
                        var value = track.TrackProperties.GetProperty(sortProperties[1]);
                        return sortProperties[1] == "AlbumTrackNumber" && int.TryParse(value, out int parsedValue)
                            ? (object)parsedValue
                            : value;
                    })
                    .ToList();

                TrackLibrary.Instance.TrackList = sortedTrackList;
            }

            if (sortedTrackList.Count > 0)
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnCollectionReordered", sortedTrackList);

        }
        public void RestoreDefaultOrder(string collectionToSort)
        {
            Debug.Log($"Setting {collectionToSort} in default order");
            var sortedTrackList = new List<Track>();

            if (collectionToSort == "library") //Library default order is by TrackID. May change this to a new field called CustomOrderIndex later
            {
                sortedTrackList = TrackLibrary.Instance.TrackList
                    .OrderBy(track => track.TrackProperties.GetProperty(Enum.GetName(typeof(LibrarySortProperties), 3))) //OrderBy defaults to ascending; Artist
                    .ThenBy(track => track.TrackProperties.GetProperty(Enum.GetName(typeof(LibrarySortProperties), 2))) //Album
                    .ThenBy(track =>
                    {
                        var value = track.TrackProperties.GetProperty(Enum.GetName(typeof(LibrarySortProperties), 0));
                        return int.TryParse(value, out int parsedValue) //AlbumTrackNumber - requires parsing from string to int so it sorts numerically and not lexiconagrahpically (i.e., 1, 10, 11, etc.)
                            ? (object)parsedValue 
                            : value; 
                    })
                    .ThenBy(track => track.TrackProperties.GetProperty(Enum.GetName(typeof(LibrarySortProperties), 1))) //Title
                    .ToList();

                TrackLibrary.Instance.TrackList = sortedTrackList;
            }

            if (sortedTrackList.Count > 0)
                ObserverManager.ObserverManager.Instance.NotifyObservers("OnCollectionReordered", sortedTrackList);
        }

        private void RefreshSorting()
        {
            SortButton buttonToSortBy = null;

            foreach (var button in UISortButtonsManager.Instance.SortButtons)
            {
                if (button.State != SortButtonState.Default)
                {
                    buttonToSortBy = button;
                }
            }

            if (buttonToSortBy == null)
            {
                RestoreDefaultOrder(UITrackListDisplayManager.Instance.TracksDisplayed);
            }
            else
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
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnNewTrackAdded" => () =>
                {
                    RefreshSorting();
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }


    }
}
