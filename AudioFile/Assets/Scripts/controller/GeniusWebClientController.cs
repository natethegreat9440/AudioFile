using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using AudioFile.Controller;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using AudioFile.Model;
using AudioFile.Utilities;
using AudioFile.View;
using TagLib;
using System.Text.RegularExpressions;
using UnityEngine.Rendering;

namespace AudioFile.Controller
{
    /// <summary>
    /// Singleton Track Library Controller in AudioFile used for finding and storing track samples and page urls from Genius.com
    /// <remarks>
    /// Members: Need to refactor so this controller doesn't have so many members with different responsibility groups
    /// Implements IAudioFileObserver, IController, and MonoBehaviour
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IController"/>
    /// </summary>

    //TODO: Need to break this controller into three smaller controllers (see different regions) for single responsibility principle adherence. GeniusWebClientController can then become an interface or abstract class for the three other controller to inherit/implement
    //TODO: Make the logic for finding samples and sampledbys cleaner and more efficient while avoiding DRY mistakes
    public class GeniusWebClientController : MonoBehaviour, IController, IAudioFileObserver
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<GeniusWebClientController> _instance = new Lazy<GeniusWebClientController>(CreateSingleton);

        // Private constructor to prevent instantiation
        private GeniusWebClientController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static GeniusWebClientController Instance => _instance.Value;

        private static GeniusWebClientController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(GeniusWebClientController).Name);

            // Add the TrackListController component to the GameObject
            return singletonObject.AddComponent<GeniusWebClientController>();
        }

        private HttpClient client;

        GeniusButton geniusButton => UIGeniusButtonManager.Instance.GeniusButton;
        SamplesText SamplesText => UISampleDisplayPanelManager.Instance.SamplesText;
        SampledByText SampledByText => UISampleDisplayPanelManager.Instance.SampledByText;
        string SelectedTrackGeniusSongID => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "GeniusSongID");

        public void Start()
        {
            ObserverManager.Instance.RegisterObserver("OnSelectedTrackSetComplete", this);
            ObserverManager.Instance.RegisterObserver("OnGeniusUrlSearchComplete", this);
            ObserverManager.Instance.RegisterObserver("OnMultipleTrackSelection", this);
        }

        public void Awake()
        {
            client = new HttpClient();
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            Debug.Log($"AudioFileUpdate: {observationType} handled at {this}");

            Action action = observationType switch
            {
                "OnSelectedTrackSetComplete" => () =>
                {
                    int trackID = (int)data;
                    SetGeniusUrlForTrack(trackID);

                    Debug.Log($"Genius Button State is: {geniusButton.State}");

                    HandleSampleDisplayTextBasedOnGeniusButtonState(trackID);
                },
                "OnGeniusUrlSearchComplete" => () =>
                {
                    int trackID = (int)data;
                    HandleSampleDisplaySearchingStates();
                    SetSelectedTrackSamplesAndSampledBysConfiguration(trackID);
                },
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }
        #region Genius Sample Info/Sample Text State Management Methods
        private void HandleSampleDisplayTextBasedOnGeniusButtonState(int trackID)
        {
            if (geniusButton.State == GeniusButtonState.Found)
            {
                HandleSampleDisplaySearchingStates();
                SetSelectedTrackSamplesAndSampledBysConfiguration(trackID);
            }
            else if (geniusButton.State == GeniusButtonState.NotFound)
            {
                SetSelectedTrackSamplesAndSampledBysConfiguration(trackID);
            }
            else if (geniusButton.State == GeniusButtonState.Searching)
            {
                HandleSampleDisplaySearchingStates();
            }
        }

        private async void SetSelectedTrackSamplesAndSampledBysConfiguration(int trackID)
        {
            Track track = TrackLibraryController.Instance.GetTrackAtID(trackID);

            if (track.TrackProperties.GetProperty(trackID, "Samples") is string samples && track.TrackProperties.GetProperty(trackID, "SampledBys") is string sampledBys)
            {
                if (track != null && (String.IsNullOrEmpty(samples) == false || String.IsNullOrEmpty(sampledBys) == false))
                {
                    List<string> sampleInfo = new List<string> { samples, sampledBys };
                    HandleCheckForSampleInfoFoundOrNotFound(sampleInfo);
                    return;
                }
            }

            var results = await FetchGeniusTrackSampleInfoAsync(SelectedTrackGeniusSongID);

            HandleFetchGeniusSampleCompletion(results, trackID);
        }

        private void HandleSampleDisplaySearchingStates()
        {
            Debug.Log($"Fetching Genius Sample info for selected track {PlaybackController.Instance.SelectedTrack}...");

            UISampleDisplayPanelManager.Instance.SetSampleTextDisplayState(SampleDisplayTextState.Searching, SamplesText);
            UISampleDisplayPanelManager.Instance.SetSampleTextDisplayState(SampleDisplayTextState.Searching, SampledByText);

            UISampleDisplayPanelManager.Instance.HandleSampleTextStateAndTextUpdate(SamplesText);
            UISampleDisplayPanelManager.Instance.HandleSampleTextStateAndTextUpdate(SampledByText);
        }

        private void HandleFetchGeniusSampleCompletion(List<string> results, int trackId)
        {

            Debug.Log("Handling sample fetch completion");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                HandleCheckForSampleInfoFoundOrNotFound(results);
                ObserverManager.Instance.NotifyObservers("OnGeniusSampleSearchComplete", trackId);
            });
        }

        private void HandleCheckForSampleInfoFoundOrNotFound(List<string> results)
        {
            if (results.Any())
            {
                if (string.IsNullOrEmpty(results[0]) == false)
                {
                    string samplesValue = results[0];
                    UISampleDisplayPanelManager.SetSelectedTrackSampleInfo("Samples", samplesValue);  //Set Samples in the Tracks table as one comma separated string for now
                    UISampleDisplayPanelManager.Instance.SetSampleTextDisplayState(SampleDisplayTextState.Found, SamplesText);
                }
                else
                {
                    UISampleDisplayPanelManager.Instance.SetSampleTextDisplayState(SampleDisplayTextState.NotFound, SamplesText);
                }

                if (results.Count > 1 && string.IsNullOrEmpty(results[1]) == false)
                {
                    string sampledBysValue = results[1];
                    UISampleDisplayPanelManager.SetSelectedTrackSampleInfo("SampledBys", sampledBysValue); //Set SampledBys in the Tracks table as one comma separated string for now
                    UISampleDisplayPanelManager.Instance.SetSampleTextDisplayState(SampleDisplayTextState.Found, SampledByText);
                }
                else
                {
                    UISampleDisplayPanelManager.Instance.SetSampleTextDisplayState(SampleDisplayTextState.NotFound, SampledByText);
                }
            }
            else
            {
                UISampleDisplayPanelManager.Instance.SetSampleTextDisplayState(SampleDisplayTextState.NotFound, SamplesText);
                UISampleDisplayPanelManager.Instance.SetSampleTextDisplayState(SampleDisplayTextState.NotFound, SampledByText);
            }

            UISampleDisplayPanelManager.Instance.HandleSampleTextStateAndTextUpdate(SamplesText);
            UISampleDisplayPanelManager.Instance.HandleSampleTextStateAndTextUpdate(SampledByText);
        }

        public async Task<List<string>> FetchGeniusTrackSampleInfoAsync(string geniusId)
        {
            try
            {
                if (client == null)
                {
                    Debug.LogError("HttpClient is NULL! Initializing now...");
                    client = new HttpClient();  // Fallback in case initialization fails
                }

                string proxyUrl = "https://audiofileproxyapi.onrender.com/api/genius/samples";
                string requestUrl = $"{proxyUrl}?geniusId={Uri.EscapeDataString(geniusId)}";

                Debug.Log($"Sending request to: {requestUrl}");

                var response = await client.GetStringAsync(requestUrl);
                Debug.Log($"Raw Response: {response}");

                JObject json = JObject.Parse(response);

                if (json == null || !json.HasValues)
                {
                    Debug.Log("FetchGeniusTrackSampleInfoAsync: JSON response is null or empty.");
                    return new List<string>();
                }

                string trackSamples = json["trackSamples"]?.ToString();
                string sampledBys = json["sampledBys"]?.ToString();

                if (string.IsNullOrEmpty(trackSamples))
                {
                    Debug.LogWarning("Warning: No samples found for track using FetchGeniusTrackSampleInfoAsync");
                }

                if (string.IsNullOrEmpty(sampledBys))
                {
                    Debug.LogWarning("Warning: No sampled by info found for track using FetchGeniusTrackSampleInfoAsync");
                }

                List<string> sampleData = new List<string> { trackSamples, sampledBys };

                return sampleData;
            }
            catch (HttpRequestException httpEx)
            {
                Debug.LogError($"HTTP Request Error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"General Error: {ex.Message}");
            }

            return new List<string>();
        }
        #endregion
        #region Genius Url Finding/Genius Button State Management Methods
        public async void SetGeniusUrlForTrack(int trackID)
        {
            Track track = TrackLibraryController.Instance.GetTrackAtID(trackID);

            if (track.TrackProperties.GetProperty(trackID, "GeniusUrl") is string geniusUrl)
            {
                if (track != null && String.IsNullOrEmpty(geniusUrl) == false)
                {
                    HandleCheckForGeniusButtonFoundOrNotFoundState(geniusUrl);
                    return;
                }
            }

            string artist = (string)track.TrackProperties.GetProperty(trackID, "Artist");
            string trackName = (string)track.TrackProperties.GetProperty(trackID, "Title");

            HandleGeniusButtonSearchingState(artist, trackName);

            var results = await FetchGeniusTrackUrlAndIDAsync(artist, trackName); // Ensures UI updates run on the main thread

            string url = results[0];
            string geniusId = results[1];


            HandleFetchGeniusUrlAndIDCompletion(trackID, url, geniusId, track);
            Debug.Log($"Genius URL for track {trackName} by {artist}: {url}");
        }

        private void HandleGeniusButtonSearchingState(string artist, string trackName)
        {
            //Debug.Log($"Fetching Genius URL for track {trackName} by {artist}...");

            UIGeniusButtonManager.Instance.SetGeniusButtonState(GeniusButtonState.Searching);

            UIGeniusButtonManager.Instance.HandleGeniusButtonStateAndTextUpdate();
        }

        private void HandleFetchGeniusUrlAndIDCompletion(int trackID, string url, string geniusId, Track track)
        {
            track.TrackProperties.SetProperty(trackID, "GeniusUrl", url);
            track.TrackProperties.SetProperty(trackID, "GeniusSongID", geniusId);

            Debug.Log("Handling Genius URL and ID fetch completion");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                HandleCheckForGeniusButtonFoundOrNotFoundState(url);
                ObserverManager.Instance.NotifyObservers("OnGeniusUrlSearchComplete", trackID);
            });
        }

        private void HandleCheckForGeniusButtonFoundOrNotFoundState(string url)
        {
            //Debug.Log($"Handling check for Genius button Found or Not Found state");

            if (url != "Not found")
            {
                UIGeniusButtonManager.Instance.SetGeniusButtonState(GeniusButtonState.Found);
            }
            else
            {
                UIGeniusButtonManager.Instance.SetGeniusButtonState(GeniusButtonState.NotFound);
            }

            UIGeniusButtonManager.Instance.HandleGeniusButtonStateAndTextUpdate();
        }

        public async Task<List<string>> FetchGeniusTrackUrlAndIDAsync(string artist, string trackName)
        {
            try
            {
                if (client == null)
                {
                    Debug.LogError("HttpClient is NULL! Initializing now...");
                    client = new HttpClient();  // Fallback in case initialization fails
                }

                string proxyUrl = "https://audiofileproxyapi.onrender.com/api/genius/search"; 
                string requestUrl = $"{proxyUrl}?artist={Uri.EscapeDataString(artist)}&trackName={Uri.EscapeDataString(trackName)}";

                Debug.Log($"Sending request to: {requestUrl}");

                var response = await client.GetStringAsync(requestUrl); 
                Debug.Log($"Raw Response: {response}");

                JObject json = JObject.Parse(response);

                string songUrl = json["songUrl"]?.ToString();
                string geniusSongId = json["geniusSongId"]?.ToString();

                if (string.IsNullOrEmpty(songUrl))
                {
                    Debug.LogWarning("Warning: No song URL found in JSON response.");
                    return new List<string> { "Not found", "Not found" };
                }

                return new List<string> { songUrl, geniusSongId };
            }
            catch (HttpRequestException httpEx)
            {
                Debug.LogError($"HTTP Request Error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"General Error: {ex.Message}");
            }

            return new List<string> { "Not found", "Not found" };
        }
#endregion
        #region FetchGeniusTrackMissingInfo Method
        public async Task<List<string>> FetchGeniusTrackMissingInfoAsync(string fileName)
        {
            try
            {
                if (client == null)
                {
                    Debug.LogError("HttpClient is NULL! Initializing now...");
                    client = new HttpClient();  // Fallback in case initialization fails
                }

                string proxyUrl = "https://audiofileproxyapi.onrender.com/api/genius/search_missing";
                string requestUrl = $"{proxyUrl}?fileName={Uri.EscapeDataString(fileName)}";

                Debug.Log($"Sending request to: {requestUrl}");

                var response = await client.GetStringAsync(requestUrl); 
                Debug.Log($"Raw Response: {response}");

                JObject json = JObject.Parse(response);

                if (json == null || !json.HasValues)
                {
                    Debug.Log("FetchGeniusTrackMissingInfoAsync: JSON response is null or empty.");
                    return new List<string>();
                }

                string trackName = json["trackName"]?.ToString();
                string artist = json["artist"]?.ToString();
                string album = json["album"]?.ToString();
                string albumTrackNumber = json["albumTrackNumber"]?.ToString();
                string songUrl = json["songUrl"]?.ToString();
                string geniusSongId = json["geniusSongId"]?.ToString();

                if (string.IsNullOrEmpty(songUrl))
                {
                    Debug.LogWarning("Warning: No song URL found in JSON response for FetchGeniusTrackMissingInfoAsync.");
                    return new List<string>();
                }

                List<string> metaData = new List<string> { trackName, artist, album, albumTrackNumber, songUrl };

                return metaData;
            }
            catch (HttpRequestException httpEx)
            {
                Debug.LogError($"HTTP Request Error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"General Error: {ex.Message}");
            }

            return new List<string>();
        }
        #endregion
        #region Non-Implemented IController Methods
        public void HandleUserRequest(object request, bool isUndo)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
