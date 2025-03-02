using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using AudioFile.Controller;

namespace AudioFile.Tests
{ 
    public class TrackSampleController_Tests
    {

        [SetUp]
        public void SetUp()
        {
            // Create a GameObject and attach SetupController
            //GameObject gameObject = new GameObject("SetupControllerTest");
            //var setupControllerTest = gameObject.AddComponent<SetupController>();
        }

        [UnityTest]
        public IEnumerator GetGeniusTrackUrlAsync_ValidQuery_ReturnsResults()
        {
            var geniusController = GeniusWebClientController.Instance;
            geniusController.Start();
            // Wait for the Start method to complete
            yield return null;

            // Act: Run the search
            //Note: Passes will all case insensitivity variations. Fairly robust
            //Task<string> searchTask = geniusController.FetchGeniusTrackUrlAsync("Mf doom", "doomsday");
            //Task<string> searchTask = geniusController.FetchGeniusTrackUrlAsync("mf doom", "doomsday");
            //Task<string> searchTask = geniusController.FetchGeniusTrackUrlAsync("MF DOOM", "DOOMSDAY"); //All caps when you spell the man's name :)
            //Task<string> searchTask = geniusController.FetchGeniusTrackUrlAsync("not DooM", "DoOmSdAy"); //Will give you a result, but it's Genius's best guess which is not Doom
            Task<string> searchTask = geniusController.FetchGeniusTrackUrlAsync("Mf DooM", "DoOmSdAy"); //All caps when you spell the man's name :)
            // Wait for the async method to complete
            yield return new WaitUntil(() => searchTask.IsCompleted);

            // Assert: Ensure we got results
            Assert.IsNotNull(searchTask.Result);
            Debug.Log($"Search Results: {searchTask.Result}");
            Assert.IsNotEmpty(searchTask.Result);
            Assert.IsTrue(searchTask.Result.Contains("https://genius.com/Mf-doom-doomsday-lyrics"));
        }

        [UnityTest]
        public IEnumerator GetGeniusTrackUrlAsync_ValidTrickyQueries_ReturnsResults()
        {
            var geniusController = GeniusWebClientController.Instance;
            geniusController.Start();
            // Wait for the Start method to complete
            yield return null;

            // Act: Run the search
            //This is trickier query since De La Soul is known as the main artist for this track (although on my fan made Doom comp album it lists Doom as the main artist)
            //Also . in co.kane is a special character dropped from the result url fed back from the site
            //Note that I may need to invent another method to deal with ? by MF DOOM lol
            Task<string> searchTask = geniusController.FetchGeniusTrackUrlAsync("MF DOOM", "Rock Co.Kane Flow ft. De La Soul");
            Task<string> searchTask2 = geniusController.FetchGeniusTrackUrlAsync("MF DOOM", "?");
            Task<string> searchTask3 = geniusController.FetchGeniusTrackUrlAsync("Kendrick Lamar", "untitled 01 | 08.19.2014.");

            // Wait for the async method to complete
            yield return new WaitUntil(() => searchTask.IsCompleted);
            yield return new WaitUntil(() => searchTask2.IsCompleted);
            yield return new WaitUntil(() => searchTask3.IsCompleted);

            // Assert: Ensure we got results
            Assert.IsNotNull(searchTask.Result);
            Debug.Log($"Search Results: {searchTask.Result}");
            Assert.IsNotEmpty(searchTask.Result);
            Assert.IsTrue(searchTask.Result.Contains("https://genius.com/De-la-soul-rock-cokane-flow-lyrics"));

            Assert.IsNotNull(searchTask2.Result);
            Debug.Log($"Search2 Results: {searchTask2.Result}");
            Assert.IsNotEmpty(searchTask2.Result);
            Assert.IsTrue(searchTask2.Result.Contains("https://genius.com/Mf-doom-question-mark-lyrics"));

            Assert.IsNotNull(searchTask3.Result);
            Debug.Log($"Search3 Results: {searchTask3.Result}");
            Assert.IsNotEmpty(searchTask3.Result);
            Assert.IsTrue(searchTask3.Result.Contains("https://genius.com/Kendrick-lamar-untitled-01-08192014-lyrics"));
        }

        [UnityTest]
        public IEnumerator FetchGeniusTrackMissingInfoAsync_ValidQueries_ReturnsResults()
        {
            var geniusController = GeniusWebClientController.Instance;
            geniusController.Start();
            // Wait for the Start method to complete
            yield return null;

            // Act: Run the search
            Task<List<string>> searchTask = geniusController.FetchGeniusTrackMissingInfoAsync("06 - Drake - Worst Behavior");
            //Task<List<string>> searchTask = geniusController.FetchGeniusTrackMissingInfoAsync("Drake - Worst Behavior");
            // Wait for the async method to complete
            yield return new WaitUntil(() => searchTask.IsCompleted);

            List<string> results = searchTask.Result;

            string trackName = results[0]; 
            string artist = results[1];
            string album = results[2];
            string albumTrackNumber = results[3];
            string songUrl = results[4];

            // Assert: Ensure we got results
            Assert.IsNotNull(searchTask.Result);
            Debug.Log($"Search Results: {searchTask.Result}");
            Assert.IsNotEmpty(searchTask.Result);
            Assert.IsTrue(trackName.Contains("Worst Behavior"));
            Assert.IsTrue(artist.Contains("Drake"));
            Assert.IsTrue(album.Contains("Nothing Was the Same"));
            Assert.IsTrue(albumTrackNumber.Contains("6"));
            Assert.IsTrue(songUrl.Contains("https://genius.com/Drake-worst-behavior-lyrics"));
        }

        [UnityTest]
        public IEnumerator FetchGeniusTrackMissingInfoAsync_ValidTrickierQuery1_ReturnsResults()
        {
            var geniusController = GeniusWebClientController.Instance;
            geniusController.Start();
            // Wait for the Start method to complete
            yield return null;

            // Act: Run the search
            Task<List<string>> searchTask = geniusController.FetchGeniusTrackMissingInfoAsync("Big Sean -  I Know (feat. Jhen Aiko) (Lyric Video)");
            
            // Wait for the async method to complete
            yield return new WaitUntil(() => searchTask.IsCompleted);

            List<string> results = searchTask.Result;

            string trackName = results[0];
            string artist = results[1];
            string album = results[2];
            string albumTrackNumber = results[3];
            string songUrl = results[4];

            // Assert: Ensure we got results
            Assert.IsNotNull(searchTask.Result);
            Debug.Log($"Search Results: {searchTask.Result}");
            Assert.IsNotEmpty(searchTask.Result);
            Assert.IsTrue(trackName.Contains("I Know"));
            Assert.IsTrue(artist.Contains("Big Sean"));
            Assert.IsTrue(album.Contains("Dark Sky Paradise (Deluxe)"));
            Assert.IsTrue(albumTrackNumber.Contains("9"));
            Assert.IsTrue(songUrl.Contains("https://genius.com/Big-sean-i-know-lyrics"));
        }

        [UnityTest]
        public IEnumerator FetchGeniusTrackMissingInfoAsync_ValidTrickierQuery2_ReturnsResults()
        {
            var geniusController = GeniusWebClientController.Instance;
            geniusController.Start();
            // Wait for the Start method to complete
            yield return null;

            // Act: Run the search
            Task<List<string>> searchTask = geniusController.FetchGeniusTrackMissingInfoAsync("Big Sean - Blessings (Extended Version _ Audio) (Explicit) ft. Drake, Kanye West");

            // Wait for the async method to complete
            yield return new WaitUntil(() => searchTask.IsCompleted);

            List<string> results = searchTask.Result;

            string trackName = results[0];
            string artist = results[1];
            string album = results[2];
            string albumTrackNumber = results[3];
            string songUrl = results[4];

            // Assert: Ensure we got results
            Assert.IsNotNull(searchTask.Result);
            Debug.Log($"Search Results: {searchTask.Result}");
            Assert.IsNotEmpty(searchTask.Result);
            Assert.IsTrue(trackName.Contains("Blessings (Extended Version)"));
            Assert.IsTrue(artist.Contains("Big Sean"));
            Assert.IsTrue(album.Contains("Blessings (Single)"));
            Assert.IsTrue(albumTrackNumber.Contains("3"));
            Assert.IsTrue(songUrl.Contains("https://genius.com/Big-sean-blessings-extended-version-lyrics"));
        }

        [UnityTest]
        public IEnumerator FetchGeniusTrackMissingInfoAsync_ValidTrickierQuery3_ReturnsResults()
        {
            var geniusController = GeniusWebClientController.Instance;
            geniusController.Start();
            // Wait for the Start method to complete
            yield return null;

            // Act: Run the search
            Task<List<string>> searchTask = geniusController.FetchGeniusTrackMissingInfoAsync("Blunt Blowin' - Lil Wayne (lyrics)");

            // Wait for the async method to complete
            yield return new WaitUntil(() => searchTask.IsCompleted);

            List<string> results = searchTask.Result;

            string trackName = results[0];
            string artist = results[1];
            string album = results[2];
            string albumTrackNumber = results[3];
            string songUrl = results[4];

            // Assert: Ensure we got results
            Assert.IsNotNull(searchTask.Result);
            Debug.Log($"Search Results: {searchTask.Result}");
            Assert.IsNotEmpty(searchTask.Result);
            Assert.IsTrue(trackName.Contains("Blunt Blowin"));
            Assert.IsTrue(artist.Contains("Lil Wayne"));
            Assert.IsTrue(album.Contains("Tha Carter IV"));
            Assert.IsTrue(albumTrackNumber.Contains("2"));
            Assert.IsTrue(songUrl.Contains("https://genius.com/Lil-wayne-blunt-blowin-lyrics"));
        }
    }
}
