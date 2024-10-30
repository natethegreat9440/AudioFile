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

        public UITrackListDisplayManager(TrackLibrary trackLibrary)
        {
            this.trackLibrary = trackLibrary;
        }

        public void Start()
        {
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackAdded", this);
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackRemoved", this);

        }


        /*public void PopulateOnLoad()
        {
            //TODO: Need a method to populate the UITrackList display based on what has been stored to memory for the track library
            //This method will listen for the OnProgramStart notification when the program is launched
            //For now it will display the full track library, but later will augment this so it displays whatever the last view filter on the library was (i.e., which playlist, album, etc.)
        }
        */

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
            yield return null; // Final yield to complete the coroutine
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
                var titleTextTransform = child.Find("UI_Track_Display/UI_Track_Title_Button/UI_Track_Title_Button_Text");
                var artistTextTransform = child.Find("UI_Track_Display/UI_Track_Artist_Button/UI_Track_Artist_Button_Text");
                var albumTextTransform = child.Find("UI_Track_Display/UI_Track_Album_Button/UI_Track_Album_Button_Text");
                var durationTextTransform = child.Find("UI_Track_Display/UI_Track_Duration_Button/UI_Track_Duration_Button_Text");

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
            if (observationType == "OnTrackAdded")
            {
                StartCoroutine(AddTrackOnUpdate(data));
            }
            else if (observationType == "OnTrackRemoved")
            {
                StartCoroutine(RemoveTrackOnUpdate(data));
            }
        }
    }
}
