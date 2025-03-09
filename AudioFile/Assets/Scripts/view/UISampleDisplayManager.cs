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

namespace AudioFile.View
{
    public class UISampleDisplayManager : MonoBehaviour, IAudioFileObserver
    {
        private static readonly Lazy<UISampleDisplayManager> _instance = new Lazy<UISampleDisplayManager>(CreateSingleton);

        // Private constructor to prevent instantiation
        private UISampleDisplayManager() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static UISampleDisplayManager Instance => _instance.Value;

        private static UISampleDisplayManager CreateSingleton()
        {
            // Check if an instance already exists in the scene
            UISampleDisplayManager existingInstance = FindObjectOfType<UISampleDisplayManager>();
            if (existingInstance != null)
            {
                return existingInstance;
            }

            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(UISampleDisplayManager).Name);
            // Add the UITrackListDisplayManager component to the GameObject
            UISampleDisplayManager instance = singletonObject.AddComponent<UISampleDisplayManager>();

            // Ensure the GameObject is not destroyed on scene load
            DontDestroyOnLoad(singletonObject);

            return instance;
        }

        readonly string samplesTextPath = "Samples_Text";
        readonly string sampledByTextPath = "Sampled_By_Text";

        private TextMeshProUGUI samplesText;
        private TextMeshProUGUI sampledByText;

        bool isMultipleTracksSelected = false;

        private static readonly object stateLock = new object();
        private GeniusButtonState lastState; // Store last state to prevent redundant updates


        string selectedTrackGeniusSongID => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "GeniusSongID");

        void Start()
        {
            samplesText = GameObject.Find(samplesTextPath).GetComponent<TextMeshProUGUI>();
            sampledByText = GameObject.Find(sampledByTextPath).GetComponent<TextMeshProUGUI>();

            ObserverManager.Instance.RegisterObserver("OnSelectedTrackSetComplete", this);
            ObserverManager.Instance.RegisterObserver("OnMultipleTrackSelection", this);
            ObserverManager.Instance.RegisterObserver("OnGeniusUrlSearchComplete", this);
        }

        void Update() //TODO: Remove this logic if the program lags or skips frames too much
        {
            if (isMultipleTracksSelected || PlaybackController.Instance.SelectedTrack == null)
            {
                samplesText.text = string.Empty;
                sampledByText.text = string.Empty;
            }
        }
        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnMultipleTrackSelection" => () =>
                {
                    isMultipleTracksSelected = true;
                },
                "OnSelectedTrackSetComplete" => () =>
                {
                    isMultipleTracksSelected = false;
                    HandleSampleDisplayTextConfiguration();
                },
                "OnGeniusUrlSearchComplete" => () =>
                {
                    HandleSampleDisplayTextConfiguration();
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }

        private void HandleSampleDisplayTextConfiguration()
        {

            string selectedTrackSamples, selectedTrackSampledBys = string.Empty;

            GetSamplesAndSampledBys(out selectedTrackSamples, out selectedTrackSampledBys);

            if (string.IsNullOrEmpty(selectedTrackSamples) || string.IsNullOrEmpty(selectedTrackSampledBys))
            {
                HandleSelectedTrackSamplesAndSampledBysConfiguration(selectedTrackSamples, selectedTrackSampledBys);
            }

            samplesText.text = "Samples: " + selectedTrackSamples;
            sampledByText.text = "Sampled By: " + selectedTrackSampledBys;
        }

        private async void HandleSelectedTrackSamplesAndSampledBysConfiguration(string selectedTrackSamples, string selectedTrackSampledBys)
        {
            var results = await GeniusWebClientController.Instance.FetchGeniusTrackSampleInfoAsync(selectedTrackGeniusSongID);

            //var results = resultsTask.Result;

            if (results.Any())
            {
                //Set Samples and SampledBys in the Tracks table
                SetSelectedTrackSamplesAndSampledBys(results);
                GetSamplesAndSampledBys(out selectedTrackSamples, out selectedTrackSampledBys);
            }
        }

        private static void SetSelectedTrackSamplesAndSampledBys(List<string> results)
        {
            PlaybackController.Instance.SelectedTrack.TrackProperties.SetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "Samples", results[0]);
            PlaybackController.Instance.SelectedTrack.TrackProperties.SetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "SampledBys", results[1]);
        }

        private static void GetSamplesAndSampledBys(out string selectedTrackSamples, out string selectedTrackSampledBys)
        {
            selectedTrackSamples = PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "Samples").ToString();
            selectedTrackSampledBys = PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "SampledBys").ToString();
        }

        //public void SetGeniusButtonState(GeniusButtonState newState)
        //{
        //    lock (stateLock)
        //    {
        //        GeniusButton.State = newState;
        //        lastState = GeniusButton.State;
        //    }
        //}

        //private IEnumerator DelayedButtonTextUpdate()
        //{
        //    yield return new WaitForEndOfFrame(); // Wait for frame to finish

        //    //Debug.Log("[DelayedButtonTextUpdate] Applying final button text update.");
        //    ManageButtonText();
        //}

        //private void ManageButtonText()
        //{
        //    //Debug.Log("Handling Genius Button Text management start");
        //    //Debug.Log($"Current state is: {GeniusButton.State} and last state is: {lastState}");
        //    string newText = GeniusButton.State switch
        //    {
        //        GeniusButtonState.Default => "Select a single track to activate Genius.com lyrics button.",
        //        GeniusButtonState.Searching => $"Searching Genius.com for {selectedTrackTitle} by {selectedTrackArtist}...",
        //        GeniusButtonState.NotFound => "Could not find Genius.com lyrics page for selected track.",
        //        GeniusButtonState.Found => $"Click Genius.com button to visit track lyrics page for {selectedTrackTitle} by {selectedTrackArtist}",
        //        _ => GeniusButton.GetComponentInChildren<Text>().text // Default case for switch: Keep current text
        //    };

        //    SetButtonText(newText);
        //    //Debug.Log("Handling Genius Button Text management end");
        //}
        //private void SetButtonText(string buttonText)
        //{
        //    Debug.Log("Setting Genius Button Text management start");

        //    var textComponent = GeniusButton.GetComponentInChildren<TextMeshProUGUI>();
        //    textComponent.text = buttonText;
        //    textComponent.ForceMeshUpdate();
        //    LayoutRebuilder.ForceRebuildLayoutImmediate(GeniusButton.GetComponent<RectTransform>());

        //    //Strongly force UI element to refresh
        //    GeniusButton.gameObject.SetActive(false);
        //    GeniusButton.gameObject.SetActive(true);

        //    //Debug.Log("Setting Genius Button Text management end");
        //}
    }
}
