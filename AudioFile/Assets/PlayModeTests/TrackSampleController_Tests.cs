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
            var geniusController = GeniusWebController.Instance;
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
            var geniusController = GeniusWebController.Instance;
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


        //[UnityTest] Whosampled forbids data scraping so this test is no longer relevant
        //public IEnumerator SearchTrackAsync_ValidQuery_ReturnsResults()
        //{
        //    var trackSampleController = GeniusWebController.Instance;
        //    // Act: Run the search
        //    //Task<List<string>> searchTask = trackSampleController.SearchTrackAsync("MF DOOM", "Doomsday");
        //    Task<List<string>> searchTask = trackSampleController.SearchTrackAsync("Doomsday MF Doom");
        //    // Wait for the async method to complete
        //    yield return new WaitUntil(() => searchTask.IsCompleted);

        //    // Assert: Ensure we got results
        //    Assert.IsNotNull(searchTask.Result); //This passes so we get some sort of result
        //    Debug.Log($"Search Results: {string.Join(", ", searchTask.Result)}"); //Looks like it's just an empty string. Mthod needs refinement to work with actual site HTML structure
        //    Assert.IsNotEmpty(searchTask.Result);
        //    //Assert.Contains("Rapp Snitch Knishes", searchTask.Result);
        //}

        //TODO: Add more test cases for transforming track titles and artist names that don't match casewise with the whosampled.com correpsonding page
    }
}
