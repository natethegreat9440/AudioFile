using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.Controller;
using AudioFile.Model;
using AudioFile.Utilities;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.Windows;
using Input = UnityEngine.Input; // Add this line to resolve ambiguity

namespace AudioFile.View
{
    /// <summary>
    /// Concrete class for updating the UI Track List display area and managing individual TrackDisplay objects.
    /// <remarks>
    /// Members: TracksDisplayed, SelectedTrackDisplays, AllTrackDisplayTransforms. Has AddTrackOnUpdate(), RemoveTrackOnUpdate(), PopulateDisplay(), UpdateDisplay() coroutines. 
    /// Has HandleTrackButtonClick(), and OnTrackDisplayDoubleClick(), GetTrackDisplayID(), GetTrackDisplay(), GetAllTrackDisplays(),
    /// GetTrackDisplayIndex(), OnTrackSelection(), DeselectAllTrackDisplays() methods.
    /// Implements Start() from MonoBehaviour. Implements AudioFileUpdate() from IAudioFileObserver. 
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IAudioFileObserver"/>
    /// </summary>

    //IMPORTANT: Make sure in the Scroll Rect component in the Inspector for the Track_List_Display Game Object the Movement type is set to Clamped otherwise content can do some weird things

    public class UITrackListDisplayManager : MonoBehaviour, IAudioFileObserver 
    {
        private static readonly Lazy<UITrackListDisplayManager> _instance = new Lazy<UITrackListDisplayManager>(CreateSingleton);

        // Private constructor to prevent instantiation
        private UITrackListDisplayManager() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static UITrackListDisplayManager Instance => _instance.Value;

        private static UITrackListDisplayManager CreateSingleton()
        {
            // Check if an instance already exists in the scene
            UITrackListDisplayManager existingInstance = FindObjectOfType<UITrackListDisplayManager>();
            if (existingInstance != null)
            {
                return existingInstance;
            }

            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(UITrackListDisplayManager).Name);
            // Add the UITrackListDisplayManager component to the GameObject
            UITrackListDisplayManager instance = singletonObject.AddComponent<UITrackListDisplayManager>();

            // Ensure the GameObject is not destroyed on scene load
            DontDestroyOnLoad(singletonObject);

            return instance;
        }

        public GameObject UI_Track_DisplayPrefab; //Attach in scene
        public Transform Track_List_DisplayViewportContent; //Attach in scene

        public string TracksDisplayed { get; set; } = "Tracks";

        private const float doubleClickThreshold = 0.3f;
        private float lastClickTime = 0f;
        private string lastButtonClicked = "";

        public readonly string titleTextPath = "UI_Track_Title_Button/UI_Track_Title_Button_Text";
        public readonly string artistTextPath = "UI_Track_Artist_Button/UI_Track_Artist_Button_Text";
        public readonly string albumTextPath = "UI_Track_Album_Button/UI_Track_Album_Button_Text";
        public readonly string durationTextPath = "UI_Track_Duration_Button/UI_Track_Duration_Button_Text";

        [HideInInspector]
        public UIContextMenu activeContextMenu;

        //Note that SelectedTrackDisplays should always be ordered from bottom to top otherwise Playback logic can get out of whack when a bulk track removal is performed
        public List<UITrackDisplay> SelectedTrackDisplays { get; private set; } = new List<UITrackDisplay>();

        public List<Transform> AllTrackDisplayTransforms { get => GetAllTrackDisplays(); } //Not currently referenced but may have some viability later
        public void Start()
        {
            ObserverManager.Instance.RegisterObserver("OnNewTrackAdded", this);
            ObserverManager.Instance.RegisterObserver("OnTrackRemoved", this);
            ObserverManager.Instance.RegisterObserver("OnActiveTrackCycled", this);
            ObserverManager.Instance.RegisterObserver("TracksDeserialized", this);
            ObserverManager.Instance.RegisterObserver("OnCollectionReordered", this);
            ObserverManager.Instance.RegisterObserver("SearchResultsFound", this);
            //ObserverManager.Instance.RegisterObserver("SearchCleared", this);
        }

        public void AudioFileUpdate(string observationType, object data)
        {
            Action action = observationType switch
            {
                "TracksDeserialized" => () =>
                {
                    if (data is List<Track> initialTrackList)
                    {
                        StartCoroutine(PopulateDisplayInitally(initialTrackList));
                    }
                },
                "OnNewTrackAdded" => () => StartCoroutine(AddTrackOnUpdate(data)),
                "OnTrackRemoved" => () =>
                {
                    ChangeActiveTrackSelectionOnTrackRemoval();
                    StartCoroutine(RemoveTrackOnUpdate(data));
                },
                "OnActiveTrackCycled" => () =>
                {
                    if (data is int activeTrackIndex)
                    {
                        ChangeActiveTrackSelectionOnTrackCycle(activeTrackIndex);
                    }
                },
                "OnCollectionReordered" => () =>
                {
                    if (data is List<int> sortedTrackIDs)
                        StartCoroutine(UpdateDisplay(sortedTrackIDs)); // TODO: have this observation get called with the full set of IDs from Tracks
                },
                "SearchResultsFound" => () =>
                {
                    if (data is List<int> searchResults)
                        StartCoroutine(UpdateDisplay(searchResults));
                },
                /*"SearchCleared" => () =>
                {
                     StartCoroutine(UpdateDisplay(searchResults));
                },*/
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled observation type: {observationType} at {this}")
            };

            action();
        }

        public void HandleTrackButtonClick(UITrackDisplay trackDisplay, string buttonType)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickThreshold && lastButtonClicked == buttonType)
            {
                OnTrackDisplayDoubleClick(trackDisplay, buttonType);
            }
            else
            {
                //TODO: Move this to a method that just handles clicks for the whole UITrackDisplay transform and delegate to that here
                OnTrackSelection(trackDisplay.TrackDisplayGameObject);
            }

            lastClickTime = Time.time;
            lastButtonClicked = buttonType;
        }

        public void OnTrackSelection(GameObject trackDisplayObject)
        {
            bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool isCtrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (isShiftPressed && SelectedTrackDisplays.Count > 0)
            {
                HandleShiftClickSelection(trackDisplayObject);
            }
            else if (isCtrlPressed)
            {
                HandleCtrlClickSelection(trackDisplayObject);
            }
            else
            {
                HandleSingleTrackSelection(trackDisplayObject);
            }
        }

        public int GetTrackDisplayID(GameObject trackDisplay)
        {
            int trackDisplayID = trackDisplay.GetComponent<UITrackDisplay>().TrackDisplayID;
            return trackDisplayID;
        }
        public int GetTrackDisplayIndex(GameObject trackDisplay)
        {
            Transform contentTransform = Track_List_DisplayViewportContent;

            for (int i = 0; i < contentTransform.childCount; i++)
            {
                if (contentTransform.GetChild(i).gameObject == trackDisplay)
                {
                    return i;
                }
            }
            return -1;
        }

        public Transform GetTrackDisplay(int providedTrackID)
        {
            Transform trackDisplayTransform = null;

            foreach (Transform child in Track_List_DisplayViewportContent)
            {
                var trackDisplayID = child.GetComponent<UITrackDisplay>().TrackDisplayID;
                if (providedTrackID == trackDisplayID)
                {
                    trackDisplayTransform = child;
                    break;
                }
            }

            return trackDisplayTransform;
        }

        public List<int> GetOrderedTrackDisplayIDs(List<int> trackDisplayIDs)
        {
            List<int> orderedTrackDisplayIDs = new List<int>();

            foreach (Transform child in Track_List_DisplayViewportContent)
            {
                var trackDisplay = child.GetComponent<UITrackDisplay>();
                if (trackDisplay != null && trackDisplayIDs.Contains(trackDisplay.TrackDisplayID))
                {
                    orderedTrackDisplayIDs.Add(trackDisplay.TrackDisplayID);
                }
            }

            return orderedTrackDisplayIDs;
        }
        private List<Transform> GetAllTrackDisplays()
        {
            var allTrackDisplayTransforms = Track_List_DisplayViewportContent.Cast<Transform>().ToList();
            return allTrackDisplayTransforms;
        }

        private IEnumerator AddTrackOnUpdate(object data)
        {
            if (data is Track providedTrack && UI_Track_DisplayPrefab != null && Track_List_DisplayViewportContent != null)
            {
                GameObject newTrackDisplayObject = Instantiate(UI_Track_DisplayPrefab, Track_List_DisplayViewportContent);
                UITrackDisplay trackDisplay = newTrackDisplayObject.GetComponent<UITrackDisplay>();

                if (trackDisplay != null)
                {
                    trackDisplay.Initialize(providedTrack, this);

                    // Force the Grid Layout Group to recalculate layout
                    LayoutRebuilder.ForceRebuildLayoutImmediate(Track_List_DisplayViewportContent.gameObject.GetComponent<RectTransform>());
                }

                //Debug.Log($"Content Size: {Track_List_DisplayViewportContent.GetComponent<RectTransform>().sizeDelta}");
                //Debug.Log($"Viewport Size: {Track_List_DisplayViewportContent.parent.GetComponent<RectTransform>().sizeDelta}");

                var layoutGroup = Track_List_DisplayViewportContent.gameObject.GetComponent<GridLayoutGroup>();
                if (layoutGroup != null)
                {
                    float totalHeight = Track_List_DisplayViewportContent.gameObject.GetComponent<RectTransform>().rect.height;
                    //Debug.Log($"Content Height: {totalHeight}");
                }
                //Debug.Log($"Child Count: {Track_List_DisplayViewportContent.childCount}");   

                yield return null;
            }
        }

        private IEnumerator RemoveTrackOnUpdate(object data)
        {
            if (data == null)
            {
                yield break;  // Exit early if data is null
            }

            Track providedTrack = data as Track;
            if (providedTrack == null || UI_Track_DisplayPrefab == null || Track_List_DisplayViewportContent == null)
            {
                yield break;  // Exit if there’s no track data or prefab reference
            }

            var providedTrackID = providedTrack.TrackID;

            Transform trackDisplayTransformToRemove = GetTrackDisplay(providedTrackID);

            if (trackDisplayTransformToRemove != null)
            {
                HandleTrackDisplayAndContextMenuRemoval(trackDisplayTransformToRemove);
                yield break;
            }

            yield break;
        }

        private void OnTrackDisplayDoubleClick(UITrackDisplay trackDisplay, string buttonType)
        {
            Debug.Log("Double-click detected on " + buttonType);
            int trackDisplayID = trackDisplay.TrackDisplayID;

            Action action = buttonType switch
            {
                "Duration" => () => PlaybackController.Instance.HandleUserRequest(new PlayCommand(trackDisplayID)),
                "Title" => () => PlaybackController.Instance.HandleUserRequest(new PlayCommand(trackDisplayID)),
                "Artist" => () =>
                {
                    Debug.Log("Search command sent with Artist filter");
                    var query = trackDisplay.ArtistButton.GetComponentInChildren<Text>().text.Trim();
                    SearchController.Instance.HandleUserRequest(new SearchCommand(query, TracksDisplayed, "Artist"));
                },
                "Album" => () =>
                {
                    Debug.Log("Search command sent with Album filter");
                    var query = trackDisplay.AlbumButton.GetComponentInChildren<Text>().text.Trim();
                    SearchController.Instance.HandleUserRequest(new SearchCommand(query, TracksDisplayed, "Album"));
                },
                _ => () => Debug.LogWarning("Unknown button type double-clicked.")
            };

            action();
        }
        private void HandleShiftClickSelection(GameObject trackDisplayObject)
        {
            int startIndex = GetTrackDisplayIndex(SelectedTrackDisplays[0].TrackDisplayGameObject);
            int endIndex = GetTrackDisplayIndex(trackDisplayObject);

            if (startIndex > endIndex) //If selecting from top to bottom invert startIndex and endIndex so for loop always executes and increments from the lowest index regardless of whether that index was selected first or last
            {
                int temp = startIndex;
                startIndex = endIndex;
                endIndex = temp;
            }

            for (int i = startIndex; i <= endIndex; i++)
            {
                var child = Track_List_DisplayViewportContent.GetChild(i).GetComponent<UITrackDisplay>();
                if (!SelectedTrackDisplays.Contains(child))
                {
                    SelectedTrackDisplays.Add(child);
                    child.IsSelected = true;
                    //child.GetComponent<Image>().color = Color.blue;
                    child.GetComponent<Image>().color = AudioFileHelpers.SetHexColor("#A8DADC"); //Soft cyan
                }
            }

            ObserverManager.Instance.NotifyObservers("OnMultipleTrackSelection", null);
        }

        private void HandleCtrlClickSelection(GameObject trackDisplayObject)
        {
            var trackDisplayComponent = trackDisplayObject.GetComponent<UITrackDisplay>();
            if (SelectedTrackDisplays.Contains(trackDisplayComponent))
            {
                SelectedTrackDisplays.Remove(trackDisplayComponent);
                trackDisplayComponent.IsSelected = false;
                trackDisplayComponent.GetComponent<Image>().color = AudioFileHelpers.SetHexColor("#F1FAEE"); //panache white
            }
            else
            {
                SelectedTrackDisplays.Add(trackDisplayComponent);
                trackDisplayComponent.IsSelected = true;
                trackDisplayComponent.GetComponent<Image>().color = AudioFileHelpers.SetHexColor("#A8DADC"); //Soft cyan
            }

            ObserverManager.Instance.NotifyObservers("OnMultipleTrackSelection", null);
        }

        private void HandleSingleTrackSelection(GameObject trackDisplayObject)
        {
            //Single track selection
            DeselectAllTrackDisplays();
            SelectedTrackDisplays.Clear();

            var trackDisplayComponent = trackDisplayObject.GetComponent<UITrackDisplay>();
            SelectedTrackDisplays.Add(trackDisplayComponent);

            trackDisplayComponent.IsSelected = true;
            trackDisplayComponent.GetComponent<Image>().color = AudioFileHelpers.SetHexColor("#A8DADC"); //Soft cyan

            int trackDisplayID = GetTrackDisplayID(trackDisplayObject);
            ObserverManager.Instance.NotifyObservers("OnSingleTrackSelected", trackDisplayID);
        }

        private void DeselectAllTrackDisplays()
        {
            foreach (Transform child in Track_List_DisplayViewportContent)
            {
                var trackDisplay = child.GetComponent<Image>();
                if (trackDisplay != null)
                {
                    child.GetComponent<UITrackDisplay>().IsSelected = false;
                    trackDisplay.color = AudioFileHelpers.SetHexColor("#F1FAEE"); //panache white
                }
            }
        }

        private IEnumerator PopulateDisplayInitally(List<Track> trackList)
        {
            foreach (var track in trackList)
            {
                Debug.Log($"Adding TrackDisplay for {track}");
                yield return AddTrackOnUpdate(track);
            }
            if (trackList == null || trackList.Count == 0)
            {
                Debug.Log("trackList is empty/null");
            }
            else
            {
                SortController.Instance.RefreshSorting(); //Essentially delegates directly to RestoreDefaultOrder, but if I do decide to have the program save Sort Button states after a session then RefreshSorting will handle accordingly
            }
            yield return null; // Ensure the final yield return is executed before notifying observers to ensure population is complete before observers are notified

            ObserverManager.Instance.NotifyObservers("TrackDisplayPopulateEnd");
        }

        private IEnumerator UpdateDisplay(List<int> sortedTrackIDs)
        {
            //Essentially this method just tries to make the display order match the order of how the provided sortedTrackList (from SortController) is ordered
            // Step 1: Create a dictionary to map track IDs to their corresponding Transform objects
            Dictionary<int, Transform> trackIDToTransformMap = new Dictionary<int, Transform>();
            MapTrackIDToTransform(trackIDToTransformMap);
            // Step 2: Iterate through the sortedTrackList and get the corresponding Transform for each track
            List<Transform> sortedTransforms = GetTransformsForTrackDisplays(sortedTrackIDs, trackIDToTransformMap);
            // Step 3: Reorder the children of Track_List_DisplayViewportContent based on the sorted list
            ReorderTrackListDisplayViewportContent(sortedTransforms);
            // Step 4: Hide any transforms where the trackDisplay.TrackID is not present in the sortedTrackIDs
            HideFilteredOutTrackDisplays(sortedTrackIDs, trackIDToTransformMap);
            yield return null;
        }

        private void HandleTrackDisplayAndContextMenuRemoval(Transform trackDisplayTransformToRemove)
        {
            //Call the TrackDisplay's DestroyContext Menu method first in case there is an open Context Menu when the Track Display is removed
            var trackDisplayGameObject = trackDisplayTransformToRemove.gameObject;
            activeContextMenu.DestroyContextMenu();
            Destroy(trackDisplayTransformToRemove.gameObject);
        }

        private void ChangeActiveTrackSelectionOnTrackCycle(int activeTrackIndex)
        {
            GameObject selectedTrackDisplay = Track_List_DisplayViewportContent.GetChild(activeTrackIndex - 1).gameObject;
            OnTrackSelection(selectedTrackDisplay);
        }

        private void ChangeActiveTrackSelectionOnTrackRemoval()
        {
            //Select the active track ID, which is now the track before the track that was removed
            if (PlaybackController.Instance.ActiveTrack != null)
            {
                int activeTrackID = PlaybackController.Instance.ActiveTrack.TrackID;
                Transform selectedTrackDisplay = GetTrackDisplay(activeTrackID);
                OnTrackSelection(selectedTrackDisplay.gameObject);
            }
        }

        private static void ReorderTrackListDisplayViewportContent(List<Transform> sortedTransforms)
        {
            for (int i = 0; i < sortedTransforms.Count; i++)
            {
                sortedTransforms[i].SetSiblingIndex(i);
            }
        }

        private static List<Transform> GetTransformsForTrackDisplays(List<int> sortedTrackIDs, Dictionary<int, Transform> trackIDToTransformMap)
        {
            List<Transform> sortedTransforms = new List<Transform>();
            foreach (var trackID in sortedTrackIDs)
            {
                if (trackIDToTransformMap.TryGetValue(trackID, out Transform trackTransform))
                {
                    sortedTransforms.Add(trackTransform);
                }
            }

            return sortedTransforms;
        }

        private void MapTrackIDToTransform(Dictionary<int, Transform> trackIDToTransformMap)
        {
            foreach (Transform child in Track_List_DisplayViewportContent)
            {
                var trackDisplay = child.GetComponent<UITrackDisplay>();
                if (trackDisplay != null)
                {
                    trackIDToTransformMap[trackDisplay.TrackDisplayID] = child;
                }
            }
        }

        private static void HideFilteredOutTrackDisplays(List<int> sortedTrackIDs, Dictionary<int, Transform> trackIDToTransformMap)
        {
            foreach (var kvp in trackIDToTransformMap)
            {
                if (!sortedTrackIDs.Contains(kvp.Key))
                {
                    kvp.Value.gameObject.SetActive(false);
                }
                else
                {
                    kvp.Value.gameObject.SetActive(true);
                }
            }
        }
    }
}
