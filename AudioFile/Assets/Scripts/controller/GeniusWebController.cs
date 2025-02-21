using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using AudioFile.Controller;
using System.Net.Http;
//using System.Text.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using AudioFile.Model;
using AudioFile.Utilities;
using AudioFile.View;
using TagLib;
using System.Text.RegularExpressions;

namespace AudioFile.Controller
{
    /// <summary>
    /// Singleton Track Library Controller in AudioFile used for finding and storing track samples from whosampled.com
    /// <remarks>
    /// Members: 
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IController"/>
    /// </summary>
    public class GeniusWebController : MonoBehaviour, IController, IAudioFileObserver
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<GeniusWebController> _instance = new Lazy<GeniusWebController>(CreateSingleton);

        // Private constructor to prevent instantiation
        private GeniusWebController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static GeniusWebController Instance => _instance.Value;

        private static GeniusWebController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(GeniusWebController).Name);

            // Add the TrackListController component to the GameObject
            return singletonObject.AddComponent<GeniusWebController>();
        }

        //Hard coding is only for temp testing before moving this controller to it's own API repo that Render will host as a proxy server
        private readonly string accessToken = "ZRu-Cuzw_S7MMiY5Myp1vuHRQ4uiAlU6jSXJijWrHyophdA-bCllxnm6_9o4zAJo";  // Replace with your Genius API Key

        private HttpClient client;

        GeniusButton geniusButton => UIGeniusButtonManager.Instance.GeniusButton;

        public void Start()
        {
            ObserverManager.Instance.RegisterObserver("OnSelectedTrackSetComplete", this);
            Initialize();
        }

        void Update()
        {
            //HandleCheckForGeniusButtonDefaultState();
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnSelectedTrackSetComplete" => () =>
                {
                    int trackID = (int)data;
                    SetGeniusUrlForTrack(trackID);
                },

                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }
        public void HandleUserRequest(object request, bool isUndo)
        {
            throw new NotImplementedException();
        }

        public async Task<string> FetchGeniusTrackUrlAsync(string artist, string trackName)
        {
            try
            {
                //TODO: Move this logic to API proxy server controller in API project so I don't have to hard code the API Key
                //Step 0: Check for ? by MFDOOM

                trackName = CheckForQuestionMarkByMFDOOM(trackName);

                // Step 1: Search for the song
                string searchUrl = $"https://api.genius.com/search?q={Uri.EscapeDataString(artist + " " + trackName)}";
                var searchResponse = await client.GetStringAsync(searchUrl);
                JObject searchJson = JObject.Parse(searchResponse);

                // Step 2: Extract song ID
                var hits = searchJson["response"]?["hits"];
                if (hits == null || hits.Type == JTokenType.Null || !hits.HasValues)
                    return "Not found";

                // Preprocess artist and track name for comparison
                string trackLower = NormalizeForUrlComparison(trackName);
                string artistLower = NormalizeForUrlComparison(artist);
                string fallbackUrl = null;

                // Step 3: Iterate over hits to find the best match
                foreach (var hit in hits)
                {
                    string url = hit["result"]?["url"]?.ToString();
                    if (string.IsNullOrEmpty(url))
                        continue;

                    string urlLower = url.ToLower();

                    // Check if URL contains both artist and track name
                    if (urlLower.Contains(artistLower) && urlLower.Contains(trackLower))
                        return url;

                    // Store the first match that contains only the track name as fallback
                    if (fallbackUrl == null && urlLower.Contains(trackLower))
                        fallbackUrl = url;
                }

                return fallbackUrl ?? "Not found";
            }
            catch (Exception ex)
            {
                Debug.Log($"Error fetching URL: {ex.Message}");
                string emptyUrl = "Not found";
                return emptyUrl;
            }

            static string CheckForQuestionMarkByMFDOOM(string trackName)
            {
                if (trackName == "?")
                {
                    trackName = "question mark";
                }

                return trackName;
            }
        }

        // Helper function to normalize strings for Genius URL comparison
        private string NormalizeForUrlComparison(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove featured artist info (ft., feat., featuring, etc.)
            input = Regex.Replace(input, @"\b(ft\.?|feat\.?|featuring|prod\.?)\b.*", "", RegexOptions.IgnoreCase).Trim();

            // Remove all special characters except letters, numbers, and spaces
            input = Regex.Replace(input.ToLower(), @"[^a-z0-9 ]", "");

            // Collapse multiple spaces (including non-breaking spaces) into a single space
            input = Regex.Replace(input, @"\s+", " ");

            // Replace spaces with hyphens
            input = Regex.Replace(input, @"\s+", "-");

            // Remove consecutive hyphens
            return input = Regex.Replace(input, @"-{2,}", "-");
        }
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

            string url = await FetchGeniusTrackUrlAsync(artist, trackName); // Ensures UI updates run on the main thread

            HandleFetchGeniusCompletion(trackID, Task.FromResult(url), track);
            Debug.Log($"Genius URL for track {trackName} by {artist}: {url}");
        }

        private void HandleFetchGeniusCompletion(int trackID, Task<string> task, Track track) 
        {
            string url = task.Result;
            track.TrackProperties.SetProperty(trackID, "GeniusUrl", url);
            Debug.Log("Handling fetch completion");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                HandleCheckForGeniusButtonFoundOrNotFoundState(url);
                ObserverManager.Instance.NotifyObservers("OnGeniusSearchComplete", trackID);
            });
        }

        private void HandleGeniusButtonSearchingState(string artist, string trackName) 
        {
            Debug.Log($"Fetching Genius URL for track {trackName} by {artist}...");
            UIGeniusButtonManager.Instance.SetGeniusButtonState(GeniusButtonState.Searching);

            UIGeniusButtonManager.Instance.HandleGeniusButtonStateAndTextUpdate();
        }

        private void HandleCheckForGeniusButtonFoundOrNotFoundState(string url) 
        {
            Debug.Log($"Handling check for Genius button Found or Not Found state");

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

        public void Initialize()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
