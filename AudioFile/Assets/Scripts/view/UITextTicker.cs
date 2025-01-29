using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using System.Collections;
using UnityEngine;
using TMPro;
using AudioFile.Utilities;
using AudioFile.Model;

namespace AudioFile.View
{
    /// <summary>
    /// Concrete class for managing/updating the UI Now Playing text area.
    /// <remarks>
    /// May want to turn this into a generic interface later on hence the generic name. If so new name for this specific concrete class might be NowPlayingTextTicker. Members: BasicMessage(), 
    /// Has a TempMessage() coroutine for user input temporary feedback messages. Implements Start() and Update() from MonoBehaviour. 
    /// Has a GetWorldRect() helper method for Update().  Implements AudioFileUpdate() from IAudioFileObserver. 
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IAudioFileObserver"/>
    /// </summary>

    //Attach this as a component to a TextMeshProUGUI object which should be a child of a Scroll View object

    //TODO: Make this into an interface if I find myself with the need for mutliple text tickers beyond for "Now Playing"
    public class UITextTicker : MonoBehaviour, IAudioFileObserver
    {
        public float scrollSpeed = 50f;
        private RectTransform textRect;
        private float startPositionX;
        private float resetPositionX;
        private bool isScrolling;
        private Rect parentRect;

        void Start()
        {
            textRect = GetComponent<RectTransform>();
            parentRect = ((RectTransform)textRect.parent).rect;
            BasicMessage();

            ObserverManager.Instance.RegisterObserver("OnActiveTrackChanged", this);
            ObserverManager.Instance.RegisterObserver("OnTrackListEnd", this);
            ObserverManager.Instance.RegisterObserver("OnTrackSkipped", this);
            ObserverManager.Instance.RegisterObserver("TrackDisplayPopulateStart", this);
            ObserverManager.Instance.RegisterObserver("TrackDisplayPopulateEnd", this);
            ObserverManager.Instance.RegisterObserver("AudioFileError", this);
            ObserverManager.Instance.RegisterObserver("BulkTrackAddStart", this);
            ObserverManager.Instance.RegisterObserver("BulkTrackAddEnd", this);
        }

        private void BasicMessage(string welcomeMessage = "Welcome to AudioFile!", bool isWelcomeScrolling = true)
        {
            if (welcomeMessage == "Welcome to AudioFile!")
            {
                //Sets the initial values of startPositionX and resetPositionX
                startPositionX = textRect.localPosition.x;
                resetPositionX = textRect.rect.width;
            }
            else
            {
                //Resets the message location so it can be fully read
                resetPositionX = parentRect.width;
                textRect.localPosition = new Vector3(resetPositionX, textRect.localPosition.y, 0);
            }

            textRect.GetComponent<TextMeshProUGUI>().text = welcomeMessage;
            isScrolling = isWelcomeScrolling;
        }

        void Update()
        {
            if (isScrolling)
            {
                textRect.localPosition += Vector3.left * scrollSpeed * Time.deltaTime;
                Vector3[] textCorners = new Vector3[4];
                textRect.GetWorldCorners(textCorners);
                float textRight = textCorners[3].x;
                Vector3[] parentCorners = new Vector3[4];
                ((RectTransform)textRect.parent).GetWorldCorners(parentCorners);
                float parentLeft = parentCorners[0].x;

                if (textRight < parentLeft)
                {
                    float resetPositionX = parentRect.width;
                    textRect.localPosition = new Vector3(resetPositionX, textRect.localPosition.y, 0);
                }
            }
            else
            {
                textRect.localPosition = new Vector3(startPositionX, textRect.localPosition.y, 0);
            }
        }

        private Rect GetWorldRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        }

        public IEnumerator TempMessage(float waitTime, string message, bool isTempMessageScrolling = false)
        {
            textRect.localPosition = new Vector3(startPositionX, textRect.localPosition.y, 0);
            textRect.GetComponent<TextMeshProUGUI>().text = message;
            isScrolling = isTempMessageScrolling;

            yield return new WaitForSecondsRealtime(waitTime);

            if (Controller.PlaybackController.Instance.ActiveTrack != null)
                textRect.GetComponent<TextMeshProUGUI>().text = Controller.PlaybackController.Instance.ActiveTrack.ToString();
            isScrolling = true;
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnActiveTrackChanged" => () =>
                {
                    textRect.localPosition = new Vector3(startPositionX, textRect.localPosition.y, 0);
                    if (Controller.PlaybackController.Instance.ActiveTrack != null)
                    {
                        textRect.GetComponent<TextMeshProUGUI>().text = Controller.PlaybackController.Instance.ActiveTrack.ToString();
                    }
                    else
                    {
                        textRect.GetComponent<TextMeshProUGUI>().text = Controller.PlaybackController.Instance.SelectedTrack.ToString(); ;
                    }
                    isScrolling = true;
                }
                ,
                "OnTrackListEnd" => () =>
                {
                    if (Controller.PlaybackController.Instance.ActiveTrackIndex == 0)
                    {
                        StartCoroutine(TempMessage(1f, "Front of playlist"));
                    }
                    else
                    {
                        StartCoroutine(TempMessage(1f, "End of playlist"));
                    }
                }
                ,
                "OnTrackSkipped" => () =>
                {
                    if (data is Track trackSkipped)
                    {
                        StartCoroutine(TempMessage(4f, $"{trackSkipped.TrackProperties.GetProperty(trackSkipped.TrackID, "Title")} skipped due to error", true));
                    }
                    else if (data is null)
                    {
                        StartCoroutine(TempMessage(4f, "Unknown track skipped due to error", true));
                    }
                    else
                    {
                        StartCoroutine(TempMessage(4f, "Track skipped due to unknown error", true));
                    }
                }
                ,
                "TrackDisplayPopulateStart" => () =>
                {
                    BasicMessage("Loading tracks...", false);
                }
                ,
                "TrackDisplayPopulateEnd" => () =>
                {
                    BasicMessage();
                }
                ,
                "BulkTrackAddStart" => () =>
                {
                    Debug.Log("Bulk track add start");
                    BasicMessage("Adding tracks...", false);
                }
                ,
                "BulkTrackAddEnd" => () =>
                {
                    Debug.Log("Bulk track add end");
                    BasicMessage();
                }
                ,
                "AudioFileError" => () =>
                {
                    string errorMessage = data as string;
                    StartCoroutine(TempMessage(8f, errorMessage, true));
                }
                ,
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }
    }
}
