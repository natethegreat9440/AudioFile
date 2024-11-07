using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioFile.Model;
using System.Collections;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.Controller;
using AudioFile.ObserverManager;


namespace AudioFile.View
{
    public class UIPlaybackButtonsManager : MonoBehaviour, IAudioFileObserver //IAudioFileObserver required method AudioFileUpdate(string observationType, object data) is last method in class
    {
        int trackDisplayIndex;

        readonly string previousButtonPath = "Playback_Controls/Previous_Button";
        readonly string playButtonPath = "Playback_Controls/Play_Pause_Button";
        readonly string nextButtonPath = "Playback_Controls/Next_Button";
        readonly string stopButtonPath = "Playback_Controls/Stop_Button";

        void Start()
        {
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackSelected", this);

            Canvas canvas = GameObject.Find("GUI_Canvas").GetComponent<Canvas>();

            //Set up Button GameObjects
            Button prevButton = canvas.transform.Find(previousButtonPath).GetComponent<Button>();
            Button playButton = canvas.transform.Find(playButtonPath).GetComponent<Button>();
            Button nextButton = canvas.transform.Find(nextButtonPath).GetComponent<Button>();
            Button stopButton = canvas.transform.Find(stopButtonPath).GetComponent<Button>();

            //Set up onClick Listeners/events for buttons

            //Previous button OnClick event setup
            if (prevButton != null)
            {
                prevButton.onClick.AddListener(() =>
                {
                    if (PlaybackController.Instance.CurrentTrack != null)
                    {
                        AudioFile.Controller.PlaybackController.Instance.HandleRequest(new PreviousItemCommand());
                    }
                });
            }

            //Next button OnClick event setup
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(() =>
                {
                    if (PlaybackController.Instance.CurrentTrack != null)
                    {
                        AudioFile.Controller.PlaybackController.Instance.HandleRequest(new NextItemCommand());
                    }
                });
            }

            //Stop button OnClick event setup
            if (stopButton != null)
            {
                stopButton.onClick.AddListener(() =>
                {
                    if (PlaybackController.Instance.CurrentTrack != null)
                    {
                        AudioFile.Controller.PlaybackController.Instance.HandleRequest(new StopCommand(trackDisplayIndex));
                    }
                });
            }

            //Play/pause button OnClick event setup. Needs some special logic to tell the controller what command is being sent
            if (playButton != null)
            {
                playButton.onClick.AddListener(() =>
                {
                    PlaybackController.Instance.HandlePlayPauseButton(trackDisplayIndex);
                });
            }
        }
        public void AudioFileUpdate(string observationType, object data)
        {
            switch (observationType)
            {
                case "OnTrackSelected":
                    trackDisplayIndex = (int)data;
                    break;
                // Add more cases here if needed
                default:
                    Debug.LogWarning($"Unhandled observation type: {observationType} at {this}");
                    break;
            }
        }
    }
}
