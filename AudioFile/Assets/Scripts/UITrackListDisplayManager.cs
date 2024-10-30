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
            //Quite a long coroutine method but I would like to have Unity be able to process these updates over multiple frames
            //Especially if the coroutine is part of a sequence of many UI updates such as adding multiple tracks in quick succession
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

            // Set the title text
            var titleText = newTrackDisplay.transform.Find("UI_Track_Display/UI_Track_Title_Button/UI_Track_Title_Button_Text")?.GetComponent<Text>();
            if (titleText != null)
            {
                titleText.text = providedTrack.TrackProperties.GetProperty("Title");
            }
            yield return null;  // Pause to let Unity process the instantiation and text update

            // Set the artist text
            var artistText = newTrackDisplay.transform.Find("UI_Track_Display/UI_Track_Artist_Button/UI_Track_Artist_Button_Text")?.GetComponent<Text>();
            if (artistText != null)
            {
                artistText.text = providedTrack.TrackProperties.GetProperty("Artist");
            }
            yield return null;

            // Set the album text
            var albumText = newTrackDisplay.transform.Find("UI_Track_Display/UI_Track_Album_Button/UI_Track_Album_Button_Text")?.GetComponent<Text>();
            if (albumText != null)
            {
                albumText.text = providedTrack.TrackProperties.GetProperty("Album");
            }
            yield return null;

            // Set the duration text
            var durationText = newTrackDisplay.transform.Find("UI_Track_Display/UI_Track_Duration_Button/UI_Track_Duration_Button_Text")?.GetComponent<Text>();
            if (durationText != null)
            {
                durationText.text = providedTrack.TrackProperties.GetProperty("Duration");
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
                Text titleText = child.Find("UI_Track_Display/UI_Track_Title_Button/UI_Track_Title_Button_Text")?.GetComponent<Text>();
                Text artistText = child.Find("UI_Track_Display/UI_Track_Artist_Button/UI_Track_Artist_Button_Text")?.GetComponent<Text>();
                Text albumText = child.Find("UI_Track_Display/UI_Track_Album_Button/UI_Track_Album_Button_Text")?.GetComponent<Text>();
                Text durationText = child.Find("UI_Track_Display/UI_Track_Duration_Button/UI_Track_Duration_Button_Text")?.GetComponent<Text>();

                if (titleText != null && titleText.text == trackTitleIdentifier && artistText.text == trackArtistIdentifier && albumText.text == trackAlbumIdentifier && durationText.text == trackDurationIdentifier)
                {
                    trackDisplayToRemove = child;
                    break;
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
