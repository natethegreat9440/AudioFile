using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Application = UnityEngine.Application;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button; // Add this line to resolve ambiguity
using AudioFile.Controller;
using System.Windows.Forms;
using Codice.CM.ConfigureHelper;
using TMPro;
using AudioFile.Utilities;
using static UnityEditorInternal.VersionControl.ListControl;

namespace AudioFile.View
{
    public class UIGeniusButtonManager : MonoBehaviour, IAudioFileObserver
    {
        private static readonly Lazy<UIGeniusButtonManager> _instance = new Lazy<UIGeniusButtonManager>(CreateSingleton);

        // Private constructor to prevent instantiation
        private UIGeniusButtonManager() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static UIGeniusButtonManager Instance => _instance.Value;

        private static UIGeniusButtonManager CreateSingleton()
        {
            // Check if an instance already exists in the scene
            UIGeniusButtonManager existingInstance = FindObjectOfType<UIGeniusButtonManager>();
            if (existingInstance != null)
            {
                return existingInstance;
            }

            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(UITrackListDisplayManager).Name);
            // Add the UITrackListDisplayManager component to the GameObject
            UIGeniusButtonManager instance = singletonObject.AddComponent<UIGeniusButtonManager>();

            // Ensure the GameObject is not destroyed on scene load
            DontDestroyOnLoad(singletonObject);

            return instance;
        }
        readonly string geniusButtonPath = "Launch_Genius_Button";

        public GeniusButton GeniusButton;

        public string targetUrl => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "GeniusUrl");

        string selectedTrackTitle => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "Title");

        string selectedTrackArtist => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "Artist");

        List<UITrackDisplay> selectedTrackDisplays => UITrackListDisplayManager.Instance.SelectedTrackDisplays;

        private GeniusButtonState lastState; // Store last state to prevent redundant updates
        void Start()
        {
            Canvas canvas = GameObject.Find("GUI_Canvas").GetComponent<Canvas>();

            GeniusButton = GameObject.Find(geniusButtonPath).GetComponent<GeniusButton>();

            if (GeniusButton != null)
            {
                ConfigureGeniusButtonOnStart();
            }

            ObserverManager.Instance.RegisterObserver("OnMultipleTrackSelection", this);
            ObserverManager.Instance.RegisterObserver("OnSelectedTrackSetComplete", this);
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnMultipleTrackSelection" => () => 
                {
                    UpdateGeniusButtonState();
                },
                "OnSelectedTrackSetComplete" => () =>
                {
                    //UpdateGeniusButtonState();
                    ManageButtonText();
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }

        void Update()
        {

            if (GeniusButton != null && GeniusButton.State == GeniusButtonState.Found)
            {
                GeniusButton.interactable = true;
            }
            else
            {
                GeniusButton.interactable = false;
            }


        }

        public void UpdateGeniusButtonState()
        {
            if (GeniusButton == null) return;

            // Update button state based on selectedTrackDisplays
            if (selectedTrackDisplays.Count > 1)
            {
                GeniusButton.State = GeniusButtonState.Default;
                ManageButtonText();
                lastState = GeniusButton.State;
                return;
            }

            // Manage button text only if the state has changed
            if (lastState != GeniusButton.State)
            {
                ManageButtonText();
                lastState = GeniusButton.State;
            }

            // Set interactability only when it changes
            GeniusButton.interactable = (GeniusButton.State == GeniusButtonState.Found);
        }

        private void ConfigureGeniusButtonOnStart()
        {
            if (GeniusButton != null)
            {
                GeniusButton.onClick.AddListener(() => Application.OpenURL(targetUrl));
            }

            GeniusButton.State = GeniusButtonState.Default;
        }

        private void ManageButtonText()
        {
            string newText = GeniusButton.State switch
            {
                GeniusButtonState.Default => "Select a single track to activate Genius.com button.",
                GeniusButtonState.Searching => $"Searching Genius.com for {selectedTrackTitle} by {selectedTrackArtist}...",
                GeniusButtonState.NotFound => "Could not find Genius.com page for selected track.",
                GeniusButtonState.Found => $"Click Genius.com button to visit track page for {selectedTrackTitle} by {selectedTrackArtist}",
                _ => GeniusButton.GetComponentInChildren<Text>().text // Default: Keep current text
            };

            SetButtonText(newText);
        }
        private void SetButtonText(string buttonText)
        {
            //GeniusButton.GetComponentInChildren<Text>().text = buttonText;
            GeniusButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
        }
    }
}
