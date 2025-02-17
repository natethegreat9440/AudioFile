using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using AudioFile.Controller;
using System.Net.Http;
using HtmlAgilityPack;

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
    public class TrackSampleController : MonoBehaviour, IController
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<TrackSampleController> _instance = new Lazy<TrackSampleController>(CreateSingleton);

        // Private constructor to prevent instantiation
        private TrackSampleController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static TrackSampleController Instance => _instance.Value;

        private static TrackSampleController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(TrackSampleController).Name);

            // Add the TrackListController component to the GameObject
            return singletonObject.AddComponent<TrackSampleController>();
        }

        private static readonly HttpClient client = new HttpClient();
        public void HandleUserRequest(object request, bool isUndo)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> SearchTrackAsync(string trackName)
        {
            string searchUrl = $"https://www.whosampled.com/search/tracks/?q={Uri.EscapeDataString(trackName)}";
            var trackResults = new List<string>();

            try
            {
                var response = await client.GetStringAsync(searchUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                // XPath to find track titles in search results
                var trackNodes = doc.DocumentNode.SelectNodes("//div[@class='trackName']/a"); //ToDO: Replace with better logic that matches site structure

                if (trackNodes != null)
                {
                    foreach (var node in trackNodes)
                    {
                        trackResults.Add(node.InnerText.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
            }

            return trackResults;
        }
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
