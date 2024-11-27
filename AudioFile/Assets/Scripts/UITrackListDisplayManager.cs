using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.Controller;
using AudioFile.Model;
using AudioFile.ObserverManager;
//using TagLib.Riff;

namespace AudioFile.View
{
    /// <summary>
    /// Concrete class for updating the UI Track List display area and managing individual TrackDisplay objects.
    /// <remarks>
    /// Needs method to instantiate the display with all tracks saved to PC memory down the road.
    /// Members: AddTrackOnUpdate() and RemoveTrackOnUpdate() coroutines. Has HandleTrackButtonClick(), and OnTrackDisplayDoubleClick().
    /// Has GetTrackDisplayIndex(), TrackSelected(), DeselectAllTrackDisplays() methods. Click interaction methods 
    /// Implements Start() from MonoBehaviour. Implements AudioFileUpdate() from IAudioFileObserver. 
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IAudioFileObserver"/>
    /// </summary>
    public class UITrackListDisplayManager : MonoBehaviour, IAudioFileObserver
    {
        //Make sure in the Scroll Rect component in the Inspector for the Track_List_Display Game Object the Movement type is set to Clamped otherwise content can do some weird things
        private static readonly Lazy<UITrackListDisplayManager> _instance = new Lazy<UITrackListDisplayManager>(CreateSingleton);

        // Private constructor to prevent instantiation
        private UITrackListDisplayManager() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static UITrackListDisplayManager Instance => _instance.Value;

        private static UITrackListDisplayManager CreateSingleton()
        {
            // Check if an instance already exists in the scene
            UITrackListDisplayManager existingInstance = FindObjectOfType<UITrackListDisplayManager>();
            if (existingInstance != null)
            {
                return existingInstance;
            }

            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(UITrackListDisplayManager).Name);
            // Add the UITrackListDisplayManager component to the GameObject
            UITrackListDisplayManager instance = singletonObject.AddComponent<UITrackListDisplayManager>();

            // Ensure the GameObject is not destroyed on scene load
            DontDestroyOnLoad(singletonObject);

            return instance;
        }

        public GameObject UI_Track_DisplayPrefab;
        public Transform Track_List_DisplayViewportContent;

        private const float doubleClickThreshold = 0.3f;
        private float lastClickTime = 0f;
        private string lastButtonClicked = "";

        public readonly string titleTextPath = "UI_Track_Title_Button/UI_Track_Title_Button_Text";
        public readonly string artistTextPath = "UI_Track_Artist_Button/UI_Track_Artist_Button_Text";
        public readonly string albumTextPath = "UI_Track_Album_Button/UI_Track_Album_Button_Text";
        public readonly string durationTextPath = "UI_Track_Duration_Button/UI_Track_Duration_Button_Text";

        public UIContextMenu activeContextMenu;

        //public bool HasSelection { get; private set; } = false;

        public void Start()
        {
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackAdded", this);
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackRemoved", this);
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnCurrentTrackCycled", this);
            ObserverManager.ObserverManager.Instance.RegisterObserver("TracksDeserialized", this);
        }

        public void HandleTrackButtonClick(UITrackDisplay trackDisplay, string buttonType)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickThreshold && lastButtonClicked == buttonType)
            {
                OnTrackDisplayDoubleClick(trackDisplay, buttonType);
            }
            else
            {
                TrackSelected(trackDisplay.TrackDisplayGameObject);
            }

            lastClickTime = Time.time;
            lastButtonClicked = buttonType;
        }

        private void OnTrackDisplayDoubleClick(UITrackDisplay trackDisplay, string buttonType)
        {
            Debug.Log("Double-click detected on " + buttonType);
            string trackDisplayID = trackDisplay.TrackDisplayID;

            Action action = buttonType switch
            {
                "Duration" => () => PlaybackController.Instance.HandleRequest(new PlayCommand(trackDisplayID)),
                "Title" => () => PlaybackController.Instance.HandleRequest(new PlayCommand(trackDisplayID)),
                /*"Artist" => () => /*Filter by artist logic here,
                "Album" => () => /*Filter by album logic here, */
                _ => () => Debug.LogWarning("Unknown button type double-clicked.")
            };

            action();
        }

        private void TrackSelected(GameObject trackDisplay)
        {
            DeselectAllTrackDisplays();
            //HasSelection = true;
            string trackDisplayID = GetTrackDisplayID(trackDisplay);
            trackDisplay.GetComponent<Image>().color = Color.blue;
            trackDisplay.GetComponent<UITrackDisplay>().IsSelected = true;
            ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackSelected", trackDisplayID);
            //Debug.Log("Did observers get notified?");
            //ObserverManager.ObserverManager.Instance.CheckObservers("OnTrackSelected");
            //ObserverManager.ObserverManager.Instance.CheckObservers("OnCurrentTrackIsDone");

        }

        private void DeselectAllTrackDisplays()
        {
            //HasSelection = false;
            foreach (Transform child in Track_List_DisplayViewportContent)
            {
                var trackDisplay = child.GetComponent<Image>();
                if (trackDisplay != null)
                {
                    child.GetComponent<UITrackDisplay>().IsSelected = false;
                    trackDisplay.color = Color.white;
                }
            }
        }
        public string GetTrackDisplayID(GameObject trackDisplay)
        {
            string trackDisplayID = trackDisplay.GetComponent<UITrackDisplay>().TrackDisplayID;
            //Debug.Log($"Getting trackDisplayID = {trackDisplayID}");
            return trackDisplayID;
        }
        public int GetTrackDisplayIndex(GameObject trackDisplay) 
        {
            Transform contentTransform = Track_List_DisplayViewportContent;

            for (int i = 0; i < contentTransform.childCount; i++)
            {
                if (contentTransform.GetChild(i).gameObject == trackDisplay)
                {
                    return i;
                }
            }
            return -1;
        }

        private Transform GetTrackDisplay(string providedTrackID)
        {
            Transform trackDisplayTransform = null;

            foreach (Transform child in Track_List_DisplayViewportContent)
            {
                var trackDisplayID = child.GetComponent<UITrackDisplay>().TrackDisplayID;
                if (providedTrackID == trackDisplayID)
                {
                    trackDisplayTransform = child;
                    break;
                }
            }

            return trackDisplayTransform;
        }

        private IEnumerator AddTrackOnUpdate(object data)
        {
            if (data is Track providedTrack && UI_Track_DisplayPrefab != null && Track_List_DisplayViewportContent != null)
            {
                GameObject newTrackDisplayObject = Instantiate(UI_Track_DisplayPrefab, Track_List_DisplayViewportContent);
                UITrackDisplay trackDisplay = newTrackDisplayObject.GetComponent<UITrackDisplay>();

                if (trackDisplay != null)
                {
                    trackDisplay.Initialize(providedTrack, this);
                }

                Debug.Log($"Content Size: {Track_List_DisplayViewportContent.GetComponent<RectTransform>().sizeDelta}");
                Debug.Log($"Viewport Size: {Track_List_DisplayViewportContent.parent.GetComponent<RectTransform>().sizeDelta}");
                Debug.Log($"Child Count: {Track_List_DisplayViewportContent.childCount}");   
                
                yield return null;
            }
        }

        private IEnumerator RemoveTrackOnUpdate(object data) //Method yet to be tested due to lack of Remove track feature currently
        {
            if (data == null)
            {
                yield break;  // Exit early if data is null
            }

            Track providedTrack = data as Track;
            if (providedTrack == null || UI_Track_DisplayPrefab == null || Track_List_DisplayViewportContent == null)
            {
                yield break;  // Exit if there’s no track data or prefab reference
            }

            var providedTrackID = providedTrack.TrackProperties.GetProperty("TrackID");

            Transform trackDisplayTransformToRemove = GetTrackDisplay(providedTrackID);

            if (trackDisplayTransformToRemove != null) //Destroy the TrackDisplay GameObject once found
            {
                //Call the TrackDisplay's DestroyContext Menu method first in case there is an open Context Menu when the
                //Track Display is removed
                //trackDisplayToRemove.GetComponent<UITrackDisplay>().DestroyContextMenu();
                var trackDisplayGameObject = trackDisplayTransformToRemove.gameObject;
                activeContextMenu.DestroyContextMenu();
                Destroy(trackDisplayTransformToRemove.gameObject);
                yield break;
            }

            yield break;
        }

        private IEnumerator PopulateOnStart(List<Track> initialTrackList)
        {
            if (initialTrackList == null || initialTrackList.Count == 0)
            {
                Debug.Log("initialTrackList is empty/null");
            }
            foreach (var track in initialTrackList)
            {
                Debug.Log($"Adding {track} on load");
                yield return AddTrackOnUpdate(track);
            }
            yield return null; // Ensure the final yield return is executed before notifying observers
            ObserverManager.ObserverManager.Instance.NotifyObservers("TrackDisplayPopulateEnd");
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "TracksDeserialized" => () =>
                {
                    if (data is List<Track> initialTrackList)
                    {
                        Debug.Log("Calling PopulateOnStart");
                        ObserverManager.ObserverManager.Instance.NotifyObservers("TrackDisplayPopulateStart");
                        StartCoroutine(PopulateOnStart(initialTrackList));

                    }
                },
                "OnTrackAdded" => () => StartCoroutine(AddTrackOnUpdate(data)),
                "OnTrackRemoved" => () =>
                {
                    //Select the current track ID, which is now the track before the track that was removed
                    string currentTrackID = PlaybackController.Instance.CurrentTrack.TrackProperties.GetProperty("TrackID");
                    Transform currentTrackDisplay = GetTrackDisplay(currentTrackID);
                    TrackSelected(currentTrackDisplay.gameObject);
                    StartCoroutine(RemoveTrackOnUpdate(data));
                },
                "OnCurrentTrackCycled" => () =>
                {
                    if (data is int currentTrackIndex)
                    {
                        GameObject currentTrackDisplay = Track_List_DisplayViewportContent.GetChild(currentTrackIndex).gameObject;
                        TrackSelected(currentTrackDisplay);
                    }
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }
    }
}
