using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Application = UnityEngine.Application;
using UnityEngine.UI;
using AudioFile.Controller;
using AudioFile.Model;
using System.Windows.Forms;
using TMPro;
using AudioFile.Utilities;
using System.Collections;
using static UnityEditorInternal.VersionControl.ListControl;
using UnityEditor.PackageManager.UI;

namespace AudioFile.View
{
    public class UISampleDisplayPanelManager : MonoBehaviour, IAudioFileObserver
    {
        private static readonly Lazy<UISampleDisplayPanelManager> _instance = new Lazy<UISampleDisplayPanelManager>(CreateSingleton);

        // Private constructor to prevent instantiation
        private UISampleDisplayPanelManager() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static UISampleDisplayPanelManager Instance => _instance.Value;

        private static UISampleDisplayPanelManager CreateSingleton()
        {
            // Check if an instance already exists in the scene
            UISampleDisplayPanelManager existingInstance = FindObjectOfType<UISampleDisplayPanelManager>();
            if (existingInstance != null)
            {
                return existingInstance;
            }

            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(UISampleDisplayPanelManager).Name);
            // Add the UITrackListDisplayManager component to the GameObject
            UISampleDisplayPanelManager instance = singletonObject.AddComponent<UISampleDisplayPanelManager>();

            // Ensure the GameObject is not destroyed on scene load
            DontDestroyOnLoad(singletonObject);

            return instance;
        }

        readonly string samplesTextPath = "Samples_Text";
        readonly string sampledByTextPath = "Sampled_By_Text";

        bool isMultipleTracksSelected = false;

        public SamplesText SamplesText;
        public SampledByText SampledByText;

        private static readonly object samplesTextStateLock = new object();
        private static readonly object sampledByTextStateLock = new object();
        private SampleDisplayTextState lastSamplesTextState; // Store last state to prevent redundant updates
        private SampleDisplayTextState lastSampledByTextState; // Store last state to prevent redundant updates

        string SelectedTrackSamples => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "Samples");
        string SelectedTrackSampledBys => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "SampledBys");
        string SelectedTrackGeniusSongID => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "GeniusSongID");

        Track selectedTrack => PlaybackController.Instance.SelectedTrack;

        Track lastSelectedTrack;
        GeniusButton geniusButton => UIGeniusButtonManager.Instance.GeniusButton;
        List<UITrackDisplay> selectedTrackDisplays => UITrackListDisplayManager.Instance.SelectedTrackDisplays;

        void Start()
        {
            SamplesText = GameObject.Find(samplesTextPath).GetComponent<SamplesText>();
            SampledByText = GameObject.Find(sampledByTextPath).GetComponent<SampledByText>();

            ObserverManager.Instance.RegisterObserver("OnSelectedTrackSetComplete", this);
            ObserverManager.Instance.RegisterObserver("OnMultipleTrackSelection", this);
            ObserverManager.Instance.RegisterObserver("OnGeniusSampleSearchComplete", this);
        }

        void Update() //TODO: Remove this logic if the program lags or skips frames too much
        {
            //if (isMultipleTracksSelected || PlaybackController.Instance.SelectedTrack == null)
            //{
            //    SetSampleTextDisplayState(SampleDisplayTextState.Default, SamplesText);
            //    SetSampleTextDisplayState(SampleDisplayTextState.Default, SampledByText);
            //}
        }
        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnMultipleTrackSelection" => () =>
                {
                    HandleSampleTextStateAndTextUpdate(SamplesText);
                    HandleSampleTextStateAndTextUpdate(SampledByText);
                },
                "OnSelectedTrackSetComplete" => () =>
                {
                    if (geniusButton.State != GeniusButtonState.NotFound)
                    {
                        StartCoroutine(DelayedSampleDisplayTextUpdate(SamplesText));
                        StartCoroutine(DelayedSampleDisplayTextUpdate(SampledByText));
                    }
                },
                "OnGeniusSampleSearchComplete" => () =>
                {
                    StartCoroutine(DelayedSampleDisplayTextUpdate(SamplesText));
                    StartCoroutine(DelayedSampleDisplayTextUpdate(SampledByText));
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }

        public IEnumerator HandleSampleTextStateAndTextUpdate(SampleDisplayText sampleDisplayText)
        {
            Debug.Log($"Handling Sample Display State/Text Update start for {sampleDisplayText.GetType().Name}. Current state for SamplesText is: {SamplesText.State} and last state is: {lastSamplesTextState}. Current state for SampledByText is: {SampledByText.State} and last state is: {lastSampledByTextState}");

            // Get the currently selected track once.
            Track currentTrack = PlaybackController.Instance.SelectedTrack;

            if (sampleDisplayText == null) yield break;

            // Update Sample Display Text state based on selectedTrackDisplays
            if (selectedTrackDisplays.Count > 1)
            {
                SetSampleTextDisplayState(SampleDisplayTextState.Default, sampleDisplayText);
                StartCoroutine(DelayedSampleDisplayTextUpdate(sampleDisplayText));

                //Set the last state to the default state
                if (sampleDisplayText is SamplesText)
                    lastSamplesTextState = SamplesText.State;
                else if (sampleDisplayText is SampledByText)
                    lastSampledByTextState = SampledByText.State;
                yield break;
            }

            // Manage sample display text only if the states or track have changed
            bool shouldUpdateSamples = (lastSamplesTextState != SamplesText.State || lastSelectedTrack != currentTrack);
            
            if (shouldUpdateSamples)
            { 
                Debug.Log("Updating SamplesText.State");
                yield return StartCoroutine(DelayedSampleDisplayTextUpdate(sampleDisplayText));

                lastSamplesTextState = SamplesText.State;

                //int selectedTrackID = PlaybackController.Instance.SelectedTrack.TrackID;
                //lastSelectedTrack = TrackLibraryController.Instance.GetTrackAtID(selectedTrackID);
                //Debug.Log($"lastSelectedTrack: {lastSelectedTrack}");
            }

            bool shouldUpdateSampledBy = (lastSampledByTextState != SampledByText.State || lastSelectedTrack != currentTrack);
            
            if (shouldUpdateSampledBy)
            {
                Debug.Log("Updating SampledByText.State");
                yield return StartCoroutine(DelayedSampleDisplayTextUpdate(sampleDisplayText));

                lastSampledByTextState = SampledByText.State;

                //int selectedTrackID = PlaybackController.Instance.SelectedTrack.TrackID;
                //lastSelectedTrack = TrackLibraryController.Instance.GetTrackAtID(selectedTrackID);
                //Debug.Log($"lastSelectedTrack: {lastSelectedTrack}");
            }

            // Update lastSelectedTrack only after processing both text objects.
            if (shouldUpdateSamples || shouldUpdateSampledBy)
            {
                lastSelectedTrack = currentTrack;
                Debug.Log($"lastSelectedTrack changed to: {lastSelectedTrack}");
            }

            Debug.Log($"Handling Sample Display State/Text Update end. Current state for SamplesText is: {SamplesText.State} and last state is: {lastSamplesTextState}. Current state for SampledByText is: {SampledByText.State} and last state is: {lastSampledByTextState}");
        }

        public static void SetSelectedTrackSampleInfo(string property, string value)
        {
            PlaybackController.Instance.SelectedTrack.TrackProperties.SetProperty(PlaybackController.Instance.SelectedTrack.TrackID, property, value);
        }

        public void SetSampleTextDisplayState(SampleDisplayTextState newState, SampleDisplayText sampleDisplayText)
        {
            if (sampleDisplayText is SamplesText)
            {
                lock (samplesTextStateLock)
                {
                    SamplesText.State = newState;
                    lastSamplesTextState = SamplesText.State;
                }
            }
            else if (sampleDisplayText is SampledByText)
            {
                lock (sampledByTextStateLock)
                {
                    SampledByText.State = newState;
                    lastSampledByTextState = SampledByText.State;
                }
            }
        }

        private IEnumerator DelayedSampleDisplayTextUpdate(SampleDisplayText sampleDisplayText)
        {
            yield return new WaitForEndOfFrame(); // Wait for frame to finish

            //Debug.Log("[DelayedSampleDisplayTextUpdate] Applying final sample text update.");
            ManageSampleDisplayText(sampleDisplayText);
        }

        private void ManageSampleDisplayText(SampleDisplayText sampleDisplayText)
        {
            Debug.Log($"Handling Sample Display Text management start for {sampleDisplayText.GetType().Name}. Current state is: {sampleDisplayText.State}");
            string newText = sampleDisplayText.State switch
            {
                SampleDisplayTextState.Default => sampleDisplayText is SamplesText ? "Samples: (Select track to find)" : "Sampled by: (Select track to find)",
                SampleDisplayTextState.Searching => "Searching Genius.com for sample info...",
                SampleDisplayTextState.NotFound => "Could not find sample info page for selected track.",
                SampleDisplayTextState.Found => sampleDisplayText is SamplesText ? $"Samples: {SelectedTrackSamples}" : $"Sampled by: {SelectedTrackSampledBys}",
                _ => sampleDisplayText.text // Default case for switch: Keep current text                
            };

            SetSampleDisplayText(newText, sampleDisplayText);
            Debug.Log("Handling Sample Display Text management end");
        }
        private void SetSampleDisplayText(string textToDisplay, SampleDisplayText sampleDisplayText)
        {
            Debug.Log($"Setting Sample Display Text management start for {sampleDisplayText.GetType().Name} to {textToDisplay}");

            if (sampleDisplayText is SamplesText)
            {
                SamplesText.text = textToDisplay;
                SamplesText.ForceMeshUpdate();
                LayoutRebuilder.ForceRebuildLayoutImmediate(SamplesText.GetComponent<RectTransform>());

                //Strongly force UI element to refresh
                SamplesText.gameObject.SetActive(false);
                SamplesText.gameObject.SetActive(true);
            }
            else if (sampleDisplayText is SampledByText)
            {
                SampledByText.text = textToDisplay;
                SampledByText.ForceMeshUpdate();
                LayoutRebuilder.ForceRebuildLayoutImmediate(SampledByText.GetComponent<RectTransform>());

                //Strongly force UI element to refresh
                SampledByText.gameObject.SetActive(false);
                SampledByText.gameObject.SetActive(true);
            }

            Debug.Log("Setting Sample Display Text management end");
        }
    }
}
