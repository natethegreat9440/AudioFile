using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.Model;
using AudioFile.ObserverManager;
using System.Collections;
using System.ComponentModel;
using AudioFile.Controller;

namespace AudioFile.View
{
    public class UITrackListDisplayManager : MonoBehaviour, IAudioFileObserver
    {
        public GameObject UI_Track_DisplayPrefab;  // Drag your Unity song entry prefab here in the Inspector
        public Transform Track_List_DisplayViewportContent;  // Drag the Unity Content GameObject of your ScrollView here
        private TrackLibrary trackLibrary;

        // Define double-click time threshold
        private const float doubleClickThreshold = 0.3f;
        private float lastClickTime = 0f;
        private string lastButtonClicked = "";

        public UITrackListDisplayManager(TrackLibrary trackLibrary)
        {
            this.trackLibrary = trackLibrary;
        }

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
        #region Click interactions
        private void TrackSelected(GameObject trackDisplay)
        {
            /*if (trackDisplay.GetComponent<Image>() != null)
            {
                Debug.Log("An image was found in the track display");
            }
            else
            {
                Debug.Log("No image was found in the track display");
            }*/

            DeselectAllTrackDisplays();
            int trackDisplayIndex = GetTrackDisplayIndex(trackDisplay);
            trackDisplay.GetComponent<Image>().color = Color.blue;  // Set selected color
            AudioFile.ObserverManager.ObserverManager.Instance.NotifyObservers("OnTrackSelected", trackDisplayIndex);
        }

        public int GetTrackDisplayIndex(GameObject trackDisplay)
        {
            // Get the Content GameObject's transform
            Transform contentTransform = Track_List_DisplayViewportContent;

            // Loop through each child of Content
            for (int i = 0; i < contentTransform.childCount; i++)
            {
                // Check if the current child is the target TrackDisplay
                if (contentTransform.GetChild(i).gameObject == trackDisplay)
                {
                    return i; // Return the index if found
                }
            }

            // If not found, return -1 (or handle the "not found" case as desired)
            return -1;
        }

        private void OnTrackDisplayButtonClick(GameObject trackDisplay, string buttonType)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            // Check if this is a double-click on the same button type
            if (timeSinceLastClick <= doubleClickThreshold && lastButtonClicked == buttonType)
            {
                // Handle double-click action with specific button type
                OnTrackDisplayDoubleClick(trackDisplay, buttonType);
            }
            else
            {
                // Handle single-click action (selection)
                TrackSelected(trackDisplay);
            }

            // Update last click time and button type
            lastClickTime = Time.time;
            lastButtonClicked = buttonType;
        }

        private void OnTrackDisplayDoubleClick(GameObject trackDisplay, string buttonType)
        {
            Debug.Log("Double-click detected on " + buttonType);
            int trackDisplayIndex = GetTrackDisplayIndex(trackDisplay);
            //int currentTrackIndex = AudioFile.Controller.PlaybackController.Instance.GetCurrentTrackIndex();

            // Call different commands based on the button type
            switch (buttonType)
            {
                case "Duration":
                // Will just play the current track for now until I can think of something cooler
                //Duration intentionally "falls through" here to save a few lines of code

                case "Title":
                    // Stop the current track before playing a new one
                    /*if (PlaybackController.Instance.CurrentTrack != null)  // Assuming you add a getter for the current track
                    {
                        PlaybackController.Instance.Stop(currentTrackIndex);
                    }*/

                    // Now play the new track
                    AudioFile.Controller.PlaybackController.Instance.HandleRequest(new PlayCommand(trackDisplayIndex));
                    break;

                case "Artist":
                    // Should filter view to all tracks by artist
                    break;

                case "Album":
                    // Should filter view to all tracks on this album
                    break;

                default:
                    Debug.LogWarning("Unknown button type double-clicked.");
                    break;
            }
        }

        private void DeselectAllTrackDisplays()
        {
            foreach (Transform child in Track_List_DisplayViewportContent)
            {
                var trackDisplay = child.GetComponent<Image>();
                if (trackDisplay != null)
                {
                    trackDisplay.color = Color.white;  // Reset to default color
                }
            }
        }

        #endregion
        #region Adding and Removing track based on updates
        private IEnumerator AddTrackOnUpdate(object data)
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

            GameObject newTrackDisplay = Instantiate(UI_Track_DisplayPrefab, Track_List_DisplayViewportContent);

            //Debug.Log(newTrackDisplay.transform.Find("UI_Track_Title_Button/UI_Track_Title_Button_Text") != null ? "Title Button and text found" : "Title Button not found");

            // Set the title text GUI_Canvas/Track_List_Display/Viewport/Content/UI_Track_Display(Clone)/
            var titleTextTransform = newTrackDisplay.transform.Find("UI_Track_Title_Button/UI_Track_Title_Button_Text");
            if (titleTextTransform != null)
            {
                var titleText = titleTextTransform.GetComponent<Text>();
                if (titleText != null)
                {
                    //Blank space precedes the text property as I don't like how close it puts the text to the edge of the button when left justified
                    titleText.text = "  " + providedTrack.TrackProperties.GetProperty("Title");
                }
            }
            yield return null;  // Pause to let Unity process the instantiation and text update

            // Set the artist text
            var artistTextTransform = newTrackDisplay.transform.Find("UI_Track_Artist_Button/UI_Track_Artist_Button_Text");
            if (artistTextTransform != null)
            {
                var artistText = artistTextTransform.GetComponent<Text>();
                if (artistText != null)
                {
                    artistText.text = "  " + providedTrack.TrackProperties.GetProperty("Artist");
                }
            }
            yield return null;

            // Set the album text
            var albumTextTransform = newTrackDisplay.transform.Find("UI_Track_Album_Button/UI_Track_Album_Button_Text");
            if (albumTextTransform != null)
            {
                var albumText = albumTextTransform.GetComponent<Text>();
                if (albumText != null)
                {
                    albumText.text = "  " + providedTrack.TrackProperties.GetProperty("Album");
                }
            }
            yield return null;

            // Set the duration text
            var durationTextTransform = newTrackDisplay.transform.Find("UI_Track_Duration_Button/UI_Track_Duration_Button_Text");
            if (durationTextTransform != null)
            {
                var durationText = durationTextTransform.GetComponent<Text>();
                if (durationText != null)
                {
                    durationText.text = "  " + providedTrack.TrackProperties.GetProperty("Duration");
                }
            }
            yield return null;
            // Instantiate and setup track display as in your code

            Button titleButton = newTrackDisplay.transform.Find("UI_Track_Title_Button").GetComponent<Button>();
            Button artistButton = newTrackDisplay.transform.Find("UI_Track_Artist_Button").GetComponent<Button>();
            Button albumButton = newTrackDisplay.transform.Find("UI_Track_Album_Button").GetComponent<Button>();
            Button durationButton = newTrackDisplay.transform.Find("UI_Track_Duration_Button").GetComponent<Button>();

            /* Disable the duration button so it cannot be clicked. 
             Uncomment out later in case I want this functionality (or lack thereof)
            if (durationButton != null)
            {
                durationButton.interactable = false;
            }*/

            // Pass a unique identifier for each button
            titleButton.onClick.AddListener(() => OnTrackDisplayButtonClick(newTrackDisplay, "Title"));
            artistButton.onClick.AddListener(() => OnTrackDisplayButtonClick(newTrackDisplay, "Artist"));
            albumButton.onClick.AddListener(() => OnTrackDisplayButtonClick(newTrackDisplay, "Album"));
            durationButton.onClick.AddListener(() => OnTrackDisplayButtonClick(newTrackDisplay, "Duration"));

            yield return null;

        }
        private IEnumerator RemoveTrackOnUpdate(object data)
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
                var titleTextTransform = child.Find("UI_Track_Title_Button/UI_Track_Title_Button_Text");
                var artistTextTransform = child.Find("UI_Track_Artist_Button/UI_Track_Artist_Button_Text");
                var albumTextTransform = child.Find("UI_Track_Album_Button/UI_Track_Album_Button_Text");
                var durationTextTransform = child.Find("UI_Track_Duration_Button/UI_Track_Duration_Button_Text");

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
#endregion
        public void AudioFileUpdate(string observationType, object data)
        {
            if (observationType == "OnTrackAdded")
            {
                StartCoroutine(AddTrackOnUpdate(data));
            }
            else if (observationType == "OnTrackRemoved")
            {
                StartCoroutine(RemoveTrackOnUpdate(data));
            }
            else if (observationType == "OnCurrentTrackCycled")
            {
                if (data is int currentTrackIndex)
                {
                    GameObject currentTrackDisplay = Track_List_DisplayViewportContent.GetChild(currentTrackIndex).gameObject;
                    TrackSelected(currentTrackDisplay);
                }
            }
        }

    }
}
