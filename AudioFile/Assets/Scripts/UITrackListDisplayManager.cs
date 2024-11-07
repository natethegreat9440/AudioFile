using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.Controller;
using AudioFile.Model;
using AudioFile.ObserverManager;

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
        private static readonly Lazy<UITrackListDisplayManager> _instance = new Lazy<UITrackListDisplayManager>(() => new UITrackListDisplayManager());
        public static UITrackListDisplayManager Instance => _instance.Value;

        public GameObject UI_Track_DisplayPrefab;
        public Transform Track_List_DisplayViewportContent;

        private const float doubleClickThreshold = 0.3f;
        private float lastClickTime = 0f;
        private string lastButtonClicked = "";

        public readonly string titleTextPath = "UI_Track_Title_Button/UI_Track_Title_Button_Text";
        public readonly string artistTextPath = "UI_Track_Artist_Button/UI_Track_Artist_Button_Text";
        public readonly string albumTextPath = "UI_Track_Album_Button/UI_Track_Album_Button_Text";
        public readonly string durationTextPath = "UI_Track_Duration_Button/UI_Track_Duration_Button_Text";

        // This class is assigned to a GameObject in scene so this method is not needed. Here it is anyway in case it is needed for future implementation
        /*private static UITrackListDisplayManager CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(UITrackListDisplayManager).Name);

            // Add the TrackListController component to the GameObject
            return singletonObject.AddComponent<UITrackListDisplayManager>();
        }*/


        public void Start()
        {
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackAdded", this);
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackRemoved", this);
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnCurrentTrackCycled", this);
        }

        /*public void PopulateOnLoad()
        {
            //TODO: Need a method to populate the UITrackList display based on what has been stored to memory for the track library
            //This method will listen for the OnProgramStart notification when the program is launched
            //For now it will display the full track library, but later will augment this so it displays whatever the last view filter on the library was (i.e., which playlist, album, etc.)
        }
        */


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
            int trackDisplayIndex = GetTrackDisplayIndex(trackDisplay.TrackDisplayGameObject);

            switch (buttonType)
            {
                case "Duration":
                case "Title":
                    AudioFile.Controller.PlaybackController.Instance.HandleRequest(new PlayCommand(trackDisplayIndex));
                    break;

                case "Artist":
                    // Filter by artist logic here
                    break;

                case "Album":
                    // Filter by album logic here
                    break;

                default:
                    Debug.LogWarning("Unknown button type double-clicked.");
                    break;
            }
        }

        private void TrackSelected(GameObject trackDisplay)
        {
            DeselectAllTrackDisplays();

            int trackDisplayIndex = GetTrackDisplayIndex(trackDisplay);
            trackDisplay.GetComponent<Image>().color = Color.blue;
            AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackSelected", trackDisplayIndex);
        }

        private void DeselectAllTrackDisplays()
        {
            foreach (Transform child in Track_List_DisplayViewportContent)
            {
                var trackDisplay = child.GetComponent<Image>();
                if (trackDisplay != null)
                {
                    trackDisplay.color = Color.white;
                }
            }
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

            // Use a unique identifier to find the specific TrackDisplay GameObject
            string trackTitleIdentifier = providedTrack.TrackProperties.GetProperty("Title");
            string trackArtistIdentifier = providedTrack.TrackProperties.GetProperty("Artist");
            string trackAlbumIdentifier = providedTrack.TrackProperties.GetProperty("Album");
            string trackDurationIdentifier = providedTrack.TrackProperties.GetProperty("Duration");

            Transform trackDisplayToRemove = null;

            // Iterate through children of Track_List_DisplayViewportContent to find the matching GameObject
            foreach (Transform child in Track_List_DisplayViewportContent)
            {
                var titleTextTransform = child.Find(titleTextPath);
                var artistTextTransform = child.Find(artistTextPath);
                var albumTextTransform = child.Find(albumTextPath);
                var durationTextTransform = child.Find(durationTextPath);

                if (titleTextTransform != null && artistTextTransform != null && albumTextTransform != null && durationTextTransform != null)
                {
                    var titleText = titleTextTransform.GetComponent<Text>();
                    var artistText = artistTextTransform.GetComponent<Text>();
                    var albumText = albumTextTransform.GetComponent<Text>();
                    var durationText = durationTextTransform.GetComponent<Text>();

                    if (titleText != null && titleText.text == trackTitleIdentifier && artistText != null && artistText.text == trackArtistIdentifier && albumText != null && albumText.text == trackAlbumIdentifier && durationText != null && durationText.text == trackDurationIdentifier)
                    {
                        trackDisplayToRemove = child;
                        break;
                    }
                }
                yield return null;  // Yield to distribute the workload over multiple frames
            }

            // Destroy the TrackDisplay GameObject if found
            if (trackDisplayToRemove != null)
            {
                Destroy(trackDisplayToRemove.gameObject);
            }
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            switch (observationType)
            {
                case "OnTrackAdded":
                    StartCoroutine(AddTrackOnUpdate(data));
                    break;
                case "OnTrackRemoved":
                    StartCoroutine(RemoveTrackOnUpdate(data));
                    break;
                case "OnCurrentTrackCycled":
                    if (data is int currentTrackIndex)
                    {
                        GameObject currentTrackDisplay = Track_List_DisplayViewportContent.GetChild(currentTrackIndex).gameObject;
                        TrackSelected(currentTrackDisplay);
                    }
                    break;
                // Add more cases here if needed
                default:
                    Debug.LogWarning($"Unhandled observation type: {observationType} at {this}");
                    break;
            }
        }
    }
}
