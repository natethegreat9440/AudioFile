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

        public void Start()
        {
            ObserverManager.Instance.RegisterObserver("OnSingleTrackSelected", this);
            Initialize();
        }
        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "OnSingleTrackSelected" => () =>
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
                // Step 1: Search for the song
                string searchUrl = $"https://api.genius.com/search?q={Uri.EscapeDataString(artist + " " + trackName)}";
                var searchResponse = await client.GetStringAsync(searchUrl);
                JObject searchJson = JObject.Parse(searchResponse);

                // Step 2: Extract song ID
                var hits = searchJson["response"]?["hits"];
                if (hits == null || hits.Type == JTokenType.Null || !hits.HasValues)
                    return "Song not found on Genius.";

                string songUrl = hits[0]["result"]?["url"]?.ToString();
                if (string.IsNullOrEmpty(songUrl)) return "Could not retrieve song URL.";

                Debug.Log("Genius Url found using Genius API!");
                return songUrl;

            }
            catch (Exception ex)
            {
                return $"Error fetching URL: {ex.Message}";
            }
        }

        public void SetGeniusUrlForTrack(int trackID)
        {
            Track track = TrackLibraryController.Instance.GetTrackAtID(trackID);
            string artist = (string)track.TrackProperties.GetProperty(trackID, "Artist");
            string trackName = (string)track.TrackProperties.GetProperty(trackID, "Title");

            Debug.Log($"Fetching Genius URL for track {trackName} by {artist}...");
            Task<string> urlTask = FetchGeniusTrackUrlAsync(artist, trackName);
            urlTask.ContinueWith(task =>
            {
                string url = task.Result;
                track.TrackProperties.SetProperty(trackID, "GeniusUrl", url);
                Debug.Log($"Genius URL for track {trackName} by {artist}: {url}");
            });
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



        //public async Task<List<string>> SearchTrackAsync(string trackArtist, string trackTitle)
        //{
        //    // Helper function to replace spaces with dashes
        //    string FormatUrl(string input) => input.Replace(" ", "-");

        //    string formattedArtist = FormatUrl(trackArtist);
        //    string formattedTitle = FormatUrl(trackTitle);

        //    string searchUrl = $"https://www.whosampled.com/{formattedArtist}/{formattedTitle}/samples/";
        //    var trackResults = new List<string>();

        //    try
        //    { 
        //        using HttpClient client = new HttpClient();
        //        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
        //        client.DefaultRequestHeaders.Referrer = new Uri("https://www.whosampled.com/");
        //        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        //        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");

        //        var response = await client.GetAsync(searchUrl); //Seems to fail here
        //        response.EnsureSuccessStatusCode();  // Throws an exception if status code is not success
        //        var responseBody = await response.Content.ReadAsStringAsync();

        //        var doc = new HtmlDocument();
        //        doc.LoadHtml(responseBody);

        //        // XPath to find all rows <tr> inside <table class="table tdata">
        //        var trackRows = doc.DocumentNode.SelectNodes("//table[contains(@class, 'table tdata')]/tbody/tr");

        //        if (trackRows != null)
        //        {
        //            foreach (var row in trackRows)
        //            {
        //                try
        //                {
        //                    // Extract Sample Title
        //                    var titleNode = row.SelectSingleNode(".//td[contains(@class, 'tdata_td2')]/a");
        //                    string sampleTitle = titleNode?.InnerText.Trim() ?? "Unknown Title";

        //                    // Extract Sample Artist Name
        //                    var artistNode = row.SelectSingleNode(".//td[contains(@class, 'tdata_td3')]/a");
        //                    string sampleArtist = artistNode?.InnerText.Trim() ?? "Unknown Artist";

        //                    // Extract Sample Year
        //                    var yearNode = row.SelectSingleNode(".//td[contains(@class, 'tdata_td3')][2]"); // Second occurrence
        //                    string sampleYear = yearNode?.InnerText.Trim() ?? "Unknown Year";

        //                    // Format result and add to list
        //                    trackResults.Add($"{sampleTitle} - {sampleArtist} ({sampleYear})");
        //                }
        //                catch (Exception innerEx)
        //                {
        //                    //Console.WriteLine($"Error parsing row: {innerEx.Message}");
        //                    Debug.LogError($"Error parsing row: {innerEx.Message}");
        //                }
        //            }
        //        }
        //    }
        //    catch (HttpRequestException httpEx)
        //    {
        //        //Console.WriteLine($"HTTP Request failed: {httpEx.Message}");
        //        Debug.Log($"HTTP Request failed: {httpEx.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        //Console.WriteLine($"Error fetching data: {ex.Message}");
        //        Debug.Log($"Error fetching data: {ex.Message}");
        //    }

        //    return trackResults;
        //}

        //public async Task<List<string>> SearchTrackAsync(string query) //Method for searching whosampled. Doesn't work due to whosampled.com's protections. You'll just get a HTTP 403 Forbidden error if you try to scrape anything
        //{
        //    var searchResults = new List<string>();

        //    // Format the search query (replace spaces with '+')
        //    string searchQuery = Uri.EscapeDataString(query.Replace(" ", "+"));
        //    string searchUrl = $"https://www.whosampled.com/search/?q={searchQuery}";

        //    try
        //    {
        //        using HttpClient client = new HttpClient();
        //        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
        //        client.DefaultRequestHeaders.Referrer = new Uri("https://www.whosampled.com/");
        //        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        //        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");

        //        var response = await client.GetAsync(searchUrl);
        //        response.EnsureSuccessStatusCode();
        //        var responseBody = await response.Content.ReadAsStringAsync();

        //        var doc = new HtmlDocument();
        //        doc.LoadHtml(responseBody);

        //        // Extract track links (Modify this if needed based on the actual site structure)
        //        var trackNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'trackTitle')]/a");

        //        if (trackNodes != null)
        //        {
        //            foreach (var node in trackNodes)
        //            {
        //                string trackTitle = node.InnerText.Trim();
        //                string trackLink = "https://www.whosampled.com" + node.GetAttributeValue("href", "#");
        //                searchResults.Add($"{trackTitle} -> {trackLink}");
        //            }
        //        }
        //    }
        //    catch (HttpRequestException httpEx)
        //    {
        //        Debug.Log($"HTTP Request failed: {httpEx.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Log($"General error: {ex.Message}");
        //    }

        //    return searchResults;
        //}

    }
}
