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
using TMPro;
using AudioFile.Utilities;
using System.Collections;

namespace AudioFile.View
{
    /// <summary>
    /// View class for controlling the Genius Button's text state
    /// <remarks>
    /// Members: GeniusButton, HandleGeniusButtonStateAndTextUpdate(), SetGeniusButtonState(), ConfigureGeniusButtonOnStart(), ManageButtonText(), SetButtonText(),
    /// and DelayedButtonTextUpdate() coroutine
    /// Implements IAudioFileObserver, and MonoBehaviour
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IAudioFileObserver"/>
    /// </summary>
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

        private static readonly object stateLock = new object();

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
            ObserverManager.Instance.RegisterObserver("OnGeniusUrlSearchComplete", this);
        }
            
        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnMultipleTrackSelection" => () => 
                {
                    HandleGeniusButtonStateAndTextUpdate();
                },
                "OnSelectedTrackSetComplete" => () =>
                {
                    StartCoroutine(DelayedButtonTextUpdate());
                },
                "OnGeniusUrlSearchComplete" => () =>
                {
                    StartCoroutine(DelayedButtonTextUpdate());
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

        public void HandleGeniusButtonStateAndTextUpdate() 
        {
            //Debug.Log("Handling Genius State/Text Update start");
            //Debug.Log($"Current state is: {GeniusButton.State} and last state is: {lastState}");

            if (GeniusButton == null) return;

            // Update button state based on selectedTrackDisplays
            if (selectedTrackDisplays.Count > 1)
            {
                SetGeniusButtonState(GeniusButtonState.Default);
                StartCoroutine(DelayedButtonTextUpdate());
                lastState = GeniusButton.State;
                return;
            }

            // Manage button text only if the state has changed
            if (lastState != GeniusButton.State)
            {
                StartCoroutine(DelayedButtonTextUpdate());
                lastState = GeniusButton.State;
            }

            // Set interactability only when it changes
            GeniusButton.interactable = (GeniusButton.State == GeniusButtonState.Found);

            //Debug.Log("Handling Genius State/Text Update end");
            //Debug.Log($"Current state is: {GeniusButton.State} and last state is: {lastState}");

        }

        public void SetGeniusButtonState(GeniusButtonState newState) 
        {
            lock (stateLock)
            {
                GeniusButton.State = newState;
                lastState = GeniusButton.State;
            }
        }

        private IEnumerator DelayedButtonTextUpdate() 
        {
            yield return new WaitForEndOfFrame(); // Wait for frame to finish

            //Debug.Log("[DelayedButtonTextUpdate] Applying final button text update.");

            ManageButtonText();
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
            //Debug.Log("Handling Genius Button Text management start");
            //Debug.Log($"Current state is: {GeniusButton.State} and last state is: {lastState}");

            string newText = GeniusButton.State switch
            {
                GeniusButtonState.Default => "Select a single track to activate Genius.com lyrics button.",
                GeniusButtonState.Searching => $"Searching Genius.com for {selectedTrackTitle} by {selectedTrackArtist}...",
                GeniusButtonState.NotFound => "Could not find Genius.com lyrics page for selected track.",
                GeniusButtonState.Found => $"Click Genius.com button to visit track lyrics page for {selectedTrackTitle} by {selectedTrackArtist}",
                _ => GeniusButton.GetComponentInChildren<Text>().text // Default case for switch: Keep current text
            };

            SetButtonText(newText);

            //Debug.Log("Handling Genius Button Text management end");
        }
        private void SetButtonText(string buttonText) 
        {
            Debug.Log("Setting Genius Button Text management start");

            var textComponent = GeniusButton.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = buttonText;
            textComponent.ForceMeshUpdate(); 
            LayoutRebuilder.ForceRebuildLayoutImmediate(GeniusButton.GetComponent<RectTransform>());

            //Strongly force UI element to refresh
            GeniusButton.gameObject.SetActive(false);
            GeniusButton.gameObject.SetActive(true);

            Debug.Log("Setting Genius Button Text management end");
        }
    }
}
