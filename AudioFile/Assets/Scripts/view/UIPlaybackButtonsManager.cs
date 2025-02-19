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
using AudioFile.Utilities;


namespace AudioFile.View
{
    /// <summary>
    /// View class for managing the Playback Button behaviour and interactions.
    /// <remarks>
    /// Members: buttons, EnableButtons(), DisableButtons(). Implements Start(), Update(), Initialize() from MonoBehaviour. Lots of button configuration done in Start().
    /// Implements AudioFileUpdate() from IAudioFileObserver.
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <see cref="IAudioFileObserver"/>
    /// </summary>
    public class UIPlaybackButtonsManager : MonoBehaviour, IAudioFileObserver //IAudioFileObserver required method AudioFileUpdate(string observationType, object data) is last method in class
    {
        int selectedTrackID;

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
            ObserverManager.Instance.RegisterObserver("OnSingleTrackSelected", this);

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

            InitializeButtons();
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

        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnSingleTrackSelected" => () => selectedTrackID = (int)data,
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }

        private void InitializeButtons()
        {
            //Set up onClick Listeners/events for buttons

            //Previous button OnClick event setup
            if (prevButton != null)
            {
                prevButton.onClick.AddListener(() =>
                {
                    if (PlaybackController.Instance.ActiveTrack != null)
                    {
                        PlaybackController.Instance.HandleUserRequest(new PreviousItemCommand());
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
                        PlaybackController.Instance.HandleUserRequest(new NextItemCommand());
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
                        PlaybackController.Instance.HandleUserRequest(new StopCommand(selectedTrackID));
                    }
                });
            }

            //Play/pause button OnClick event setup. Needs some special logic to tell the controller what command is being sent
            if (playButton != null)
            {
                playButton.onClick.AddListener(() =>
                {
                    PlaybackController.Instance.HandlePlayPauseButton(selectedTrackID);
                });
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
    }
}
