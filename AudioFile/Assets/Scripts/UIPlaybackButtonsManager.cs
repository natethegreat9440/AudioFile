﻿using System;
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
        string trackDisplayID;

        readonly string previousButtonPath = "Playback_Controls/Previous_Button";
        readonly string playButtonPath = "Playback_Controls/Play_Pause_Button";
        readonly string nextButtonPath = "Playback_Controls/Next_Button";
        readonly string stopButtonPath = "Playback_Controls/Stop_Button";

        Button prevButton;
        Button playButton;
        Button nextButton;
        Button stopButton;

        List<Button> buttons;

        void Start()
        {
            ObserverManager.ObserverManager.Instance.RegisterObserver("OnSingleTrackSelected", this);

            Canvas canvas = GameObject.Find("GUI_Canvas").GetComponent<Canvas>();

            //Set up Button GameObjects
            prevButton = canvas.transform.Find(previousButtonPath).GetComponent<Button>();
            playButton = canvas.transform.Find(playButtonPath).GetComponent<Button>();
            nextButton = canvas.transform.Find(nextButtonPath).GetComponent<Button>();
            stopButton = canvas.transform.Find(stopButtonPath).GetComponent<Button>();

            // Initialize the buttons list after the buttons are assigned
            buttons = new List<Button>()
            {
                prevButton,
                playButton,
                nextButton,
                stopButton,
            };

            //Set up onClick Listeners/events for buttons

            //Previous button OnClick event setup
            if (prevButton != null)
            {
                prevButton.onClick.AddListener(() =>
                {
                    if (PlaybackController.Instance.ActiveTrack != null)
                    {
                        PlaybackController.Instance.HandleRequest(new PreviousItemCommand());
                    }
                });
            }

            //Next button OnClick event setup
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(() =>
                {
                    if (PlaybackController.Instance.ActiveTrack != null)
                    {
                        PlaybackController.Instance.HandleRequest(new NextItemCommand());
                    }
                });
            }

            //Stop button OnClick event setup
            if (stopButton != null)
            {
                stopButton.onClick.AddListener(() =>
                {
                    if (PlaybackController.Instance.ActiveTrack != null)
                    {
                        PlaybackController.Instance.HandleRequest(new StopCommand(trackDisplayID));
                    }
                });
            }

            //Play/pause button OnClick event setup. Needs some special logic to tell the controller what command is being sent
            if (playButton != null)
            {
                playButton.onClick.AddListener(() =>
                {
                    PlaybackController.Instance.HandlePlayPauseButton(trackDisplayID);
                });
            }
        }

        void Update()
        {
            if (UITrackListDisplayManager.Instance.SelectedTrackDisplays.Count > 1)
            {
                DisableButtons();
            }
            else
            {
                EnableButtons();
            }
        }

        private void EnableButtons()
        {
            foreach (var button in buttons)
            {
                button.interactable = true;
            }
        }

        private void DisableButtons()
        {
            foreach (var button in buttons)
            {
                button.interactable = false;
            }
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnSingleTrackSelected" => () => trackDisplayID = (string)data,
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }
    }
}
