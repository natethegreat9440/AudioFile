﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using System.Collections;
using UnityEngine;
using TMPro;
using AudioFile.ObserverManager;

namespace AudioFile.View
{
    /// <summary>
    /// Concrete class for managing/updating the UI Now Playing text area.
    /// <remarks>
    /// May want to turn this into a generic interface later on hence the generic name. If so new name for this specific concrete class might be NowPlayingTextTicker. Members:
    /// Has a QuickMessage() coroutine for user input temporary feedback messages. Implements Start() and Update() from MonoBehaviour. 
    /// Has a GetWorldRect() helper method for Update().  Implements AudioFileUpdate() from IAudioFileObserver. 
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IAudioFileObserver"/>
    /// </summary>

    //Attach this as a component to a TextMeshProUGUI object which should be a child of a Scroll View object

    //TODO: Make this into an interface if I find myself with the need for mutliple text tickers beyond for "Now Playing"
    public class UITextTicker : MonoBehaviour, IAudioFileObserver //IAudioFileObserver required method AudioFileUpdate(string observationType, object data) is last method in class
    {
        public float scrollSpeed = 50f; // Adjust speed here. 50 is a good default
        private RectTransform textRect;
        private float startPositionX;
        private float resetPositionX;
        private bool isScrolling;


        void Start()
        {
            // Set up scrolling text and welcome message
            textRect = GetComponent<RectTransform>();
            startPositionX = textRect.localPosition.x;
            resetPositionX = textRect.rect.width; // Width of the text

            //TODO: Have this choose a random humorous phrase from a phrase bank (List of strings)
            textRect.GetComponent<TextMeshProUGUI>().text = "Welcome to AudioFile!";
            isScrolling = true;

            //Register for these observations
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnCurrentTrackChanged", this);
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackListEnd", this);
        }

        void Update()
        {
            if (isScrolling)
            {
                // Move text to the left
                textRect.localPosition += Vector3.left * scrollSpeed * Time.deltaTime;

                // Check if the TextMeshPro RectTransform is out of bounds of the parent Content RectTransform
                Rect parentRect = ((RectTransform)textRect.parent).rect;
                Vector3[] textCorners = new Vector3[4];
                textRect.GetWorldCorners(textCorners);

                // Get the left and right bounds of the TextMeshPro object
                float textLeft = textCorners[0].x;
                float textRight = textCorners[3].x;

                // Get the left boundary of the parent Content in world space
                Vector3[] parentCorners = new Vector3[4];
                ((RectTransform)textRect.parent).GetWorldCorners(parentCorners);
                float parentLeft = parentCorners[0].x;

                // Check if the right side of the text has passed the left side of the Content
                if (textRight < parentLeft)
                {
                    // Reset the text position to the right side of the viewport
                    float resetPositionX = parentRect.width;
                    textRect.localPosition = new Vector3(resetPositionX, textRect.localPosition.y, 0);
                }
            }
            else
            {
                textRect.localPosition = new Vector3(startPositionX, textRect.localPosition.y, 0);
            }
        }

        // Helper method to get the world space Rect of a RectTransform
        private Rect GetWorldRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        }

        private IEnumerator QuickMessage(float waitTime, string message)
        {
            //This is the Quick message that will appear on the ticker for the provided wait time
            textRect.GetComponent<TextMeshProUGUI>().text = message;
            isScrolling = false;

            yield return new WaitForSeconds(waitTime);

            //After waiting the coroutine resets the beahvior. Moving this outside of the coroutine will now work.
            //If this method is to be abstracted then it needs additional parameter(s) to specify reset behavior
            textRect.GetComponent<TextMeshProUGUI>().text = AudioFile.Controller.PlaybackController.Instance.CurrentTrack.ToString();
            isScrolling = true;
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            switch (observationType)
            {
                case "OnCurrentTrackChanged":
                    textRect.localPosition = new Vector3(startPositionX, textRect.localPosition.y, 0);
                    textRect.GetComponent<TextMeshProUGUI>().text = AudioFile.Controller.PlaybackController.Instance.CurrentTrack.ToString();
                    isScrolling = true;
                    break;
                case "OnTrackListEnd":
                    if (AudioFile.Controller.PlaybackController.Instance.GetCurrentTrackIndex() == 0)
                    {
                        StartCoroutine(QuickMessage(1f, "Front of playlist"));
                        break;
                    }
                    else
                    {
                        StartCoroutine(QuickMessage(1f, "End of playlist"));
                        break;
                    }
                // Add more cases here if needed
                default:
                    Debug.LogWarning($"Unhandled observation type: {observationType} at {this}");
                    break;
            }
        }

    }
}