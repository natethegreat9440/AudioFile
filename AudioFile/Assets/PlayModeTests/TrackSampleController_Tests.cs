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
        public IEnumerator SearchTrackAsync_ValidQuery_ReturnsResults()
        {
            var trackSampleController = TrackSampleController.Instance;
            // Act: Run the search
            //Task<List<string>> searchTask = trackSampleController.SearchTrackAsync("MF DOOM", "Doomsday");
            Task<List<string>> searchTask = trackSampleController.SearchTrackAsync("Doomsday MF Doom");
            // Wait for the async method to complete
            yield return new WaitUntil(() => searchTask.IsCompleted);

            // Assert: Ensure we got results
            Assert.IsNotNull(searchTask.Result); //This passes so we get some sort of result
            Debug.Log($"Search Results: {string.Join(", ", searchTask.Result)}"); //Looks like it's just an empty string. Mthod needs refinement to work with actual site HTML structure
            Assert.IsNotEmpty(searchTask.Result);
            //Assert.Contains("Rapp Snitch Knishes", searchTask.Result);
        }

        //TODO: Add more test cases for transforming track titles and artist names that don't match casewise with the whosampled.com correpsonding page
    }
}
