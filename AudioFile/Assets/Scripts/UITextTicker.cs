using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using System.Collections;
using UnityEngine;
using TMPro;
using AudioFile.ObserverManager;
using AudioFile.Model;
using Unity.PlasticSCM.Editor.WebApi;

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
            DisplayWelcome();

            //Register for these observations
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnCurrentTrackChanged", this);
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackListEnd", this);
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackSkipped", this);
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackRemoved", this);
            ObserverManager.ObserverManager.Instance.RegisterObserver("TrackDisplayPopulateStart", this);
            ObserverManager.ObserverManager.Instance.RegisterObserver("TrackDisplayPopulateEnd", this);
        }

        private void DisplayWelcome(string welcomeMessage = "Welcome to AudioFile!", bool isWelcomeScrolling = true)
        {
            // Set up scrolling text and welcome message
            textRect = GetComponent<RectTransform>();
            startPositionX = textRect.localPosition.x;
            resetPositionX = textRect.rect.width; // Width of the text

            //TODO: Have this choose a random humorous phrase from a phrase bank (List of strings)
            textRect.GetComponent<TextMeshProUGUI>().text = welcomeMessage;
            isScrolling = isWelcomeScrolling;
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

        private IEnumerator QuickMessage(float waitTime, string message, bool isQuickMessageScrolling = false)
        {
            //This is the Quick message that will appear on the ticker for the provided wait time
            textRect.GetComponent<TextMeshProUGUI>().text = message;
            isScrolling = isQuickMessageScrolling;

            yield return new WaitForSeconds(waitTime);

            //After waiting the coroutine resets the beahvior. Moving this outside of the coroutine will now work.
            //If this method is to be abstracted then it needs additional parameter(s) to specify reset behavior
            if (Controller.PlaybackController.Instance.CurrentTrack != null)
                textRect.GetComponent<TextMeshProUGUI>().text = Controller.PlaybackController.Instance.CurrentTrack.ToString();
            isScrolling = true;
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnCurrentTrackChanged" => () =>
                {
                    textRect.localPosition = new Vector3(startPositionX, textRect.localPosition.y, 0);
                    textRect.GetComponent<TextMeshProUGUI>().text = Controller.PlaybackController.Instance.CurrentTrack.ToString();
                    isScrolling = true;
                },
                "OnTrackListEnd" => () =>
                {
                    if (Controller.PlaybackController.Instance.CurrentTrackIndex == 0)
                    {
                        StartCoroutine(QuickMessage(1f, "Front of playlist"));
                    }
                    else
                    {
                        StartCoroutine(QuickMessage(1f, "End of playlist"));
                    }
                },
                "OnTrackRemoved" => () =>
                {
                    if (data is Track trackRemoved)
                    {
                        StartCoroutine(QuickMessage(4f, $"{trackRemoved.TrackProperties.GetProperty("Title")} removed from library", true));
                    }
                }
                ,
                "OnTrackSkipped" => () =>
                {
                    if (data is Track trackSkipped)
                    {
                        StartCoroutine(QuickMessage(4f, $"{trackSkipped.TrackProperties.GetProperty("Title")} skipped due to error", true));
                    }
                    else if (data is null)
                    {
                        StartCoroutine(QuickMessage(4f, "Unknown track skipped due to error", true));
                    }
                    else
                    {
                        StartCoroutine(QuickMessage(4f, "Track skipped due to unknown error", true));
                    }
                },
                "TrackDisplayPopulateStart" => () =>
                {
                    DisplayWelcome("Loading tracks...", false);
                },
                "TrackDisplayPopulateEnd" => () =>
                {
                    DisplayWelcome();
                }
                ,
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }

    }
}
