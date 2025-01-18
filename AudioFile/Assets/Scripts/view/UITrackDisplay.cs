using UnityEngine;
using UnityEngine.UI;
using AudioFile.Model;
using TagLib.Riff;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace AudioFile.View
{
    /// <summary>
    /// Concrete class for a UI Track Display item which holds Title, Artist, Album, and Duration buttons.
    /// <remarks>
    /// This needs to be attached as a component to the UI_Track_Display_Prefab
    /// Members: Initialize(), SetTrackData(), SetText(), InitializeButtons(), OnButtonClicked().
    /// OnButtonClicked() calls HandleTrackButtonClick() from the UITrackListDisplayManager.
    /// Implements Awake() from MonoBehaviour.
    /// </remarks>
    /// <see cref="UITrackListDisplayManager"/>
    /// <seealso cref="MonoBehaviour"/>
    /// </summary>

    public class UITrackDisplay : MonoBehaviour, IPointerClickHandler
    {
        public GameObject TrackDisplayGameObject;
        public Button TitleButton { get; private set; } 
        public Button ArtistButton { get; private set; } 
        public Button AlbumButton { get; private set; } 
        public Button DurationButton { get; private set; }

        private UITrackListDisplayManager listDisplayManager;
        public GameObject ContextMenuPrefab; //Will need to drag this prefab into UI_Track_Display_Prefab
                                             //TODO: Add an AllPlaylist and AllPlaylistFolder collection (or something along those lines) reference
                                             //so whenever the user right clicks on a track, it populates the context menu with the appropriate options
                                             // these references here should just get from the global reference when called internally here

        public UIContextMenu ContextMenuInstance { get; private set; }

        public int TrackDisplayID { get; private set; }

        public bool IsSelected { get; set; } = false; //TrackListDisplayManager gets and sets these for individual instances

        private void Awake()
        {
            // Set TrackDisplayGameObject to the current GameObject this script is attached to
            TrackDisplayGameObject = this.gameObject;
        }
        public void Initialize(Track trackData, UITrackListDisplayManager manager)
        {
            listDisplayManager = manager;

            //Set TrackDisplayID to match TrackID so these can be compared later with LINQ queries
            TrackDisplayID = trackData.TrackID;
            SetTrackData(trackData);
            InitializeButtons();

        }

        private void SetTrackData(Track trackData)
        {
            SetText(listDisplayManager.titleTextPath, trackData.TrackProperties.GetProperty(trackData.TrackID, "Title"));
            SetText(listDisplayManager.artistTextPath, trackData.TrackProperties.GetProperty(trackData.TrackID, "Artist"));
            SetText(listDisplayManager.albumTextPath, trackData.TrackProperties.GetProperty(trackData.TrackID, "Album"));
            SetText(listDisplayManager.durationTextPath, trackData.TrackProperties.GetProperty(trackData.TrackID, "Duration"));
        }

        private void SetText(string path, string value)
        {
            var textTransform = transform.Find(path);
            if (textTransform != null)
            {
                var textComponent = textTransform.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = "  " + value; // Adding some space for aesthetic reasons.
                }
            }
        }

        public string GetText(string path)
        {
            var textTransform = transform.Find(path);
            if (textTransform != null)
            {
                var textComponent = textTransform.GetComponent<Text>();
                if (textComponent != null)
                {
                    return textComponent.text;
                }
            }
            return "";
        }

        private void InitializeButtons()
        {
            TitleButton = transform.Find("UI_Track_Title_Button").GetComponent<Button>();
            ArtistButton = transform.Find("UI_Track_Artist_Button").GetComponent<Button>();
            AlbumButton = transform.Find("UI_Track_Album_Button").GetComponent<Button>();
            DurationButton = transform.Find("UI_Track_Duration_Button").GetComponent<Button>();

            /* Disable the duration button so it cannot be clicked. 
            Uncomment out later in case I want this functionality (or lack thereof)
            if (DurationButton != null)
            {
                DurationButton.interactable = false;
            }*/

            // Set up onClick events for Playback buttons
            if (TitleButton != null) TitleButton.onClick.AddListener(() => OnButtonClicked("Title"));
            if (ArtistButton != null) ArtistButton.onClick.AddListener(() => OnButtonClicked("Artist"));
            if (AlbumButton != null) AlbumButton.onClick.AddListener(() => OnButtonClicked("Album"));
            if (DurationButton != null) DurationButton.onClick.AddListener(() => OnButtonClicked("Duration"));
        }


        private void OnButtonClicked(string buttonType)
        {
            listDisplayManager.HandleTrackButtonClick(this, buttonType);
            Debug.Log("Button clicked: " + buttonType);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log("Pointer click detected");
            // Check for right-click
            if (eventData.button == PointerEventData.InputButton.Right && IsSelected)
            {

                Debug.Log($"Right-click detected on {transform.Find(UITrackListDisplayManager.Instance.titleTextPath).GetComponent<Text>()} - {transform.Find(UITrackListDisplayManager.Instance.titleTextPath).GetComponent<Text>()}");
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                RectTransform canvasRectTransform = mainCanvas.GetComponent<RectTransform>();

                if (ContextMenuPrefab != null && canvasRectTransform != null)
                {
                    GameObject newContextMenuInstance = Instantiate(ContextMenuPrefab, canvasRectTransform);
                    UIContextMenu contextMenu = newContextMenuInstance.GetComponent<UIContextMenu>();
                    if (contextMenu != null)
                    {
                        Vector2 displayPosition = eventData.position;

                        var trackDisplayIDList = new List<string>();

                        foreach (var trackDisplay in listDisplayManager.SelectedTrackDisplays)
                        {
                            trackDisplayIDList.Add(trackDisplay.TrackDisplayID);
                        }

                        ContextMenuInstance = contextMenu.Initialize(trackDisplayIDList, eventData.position, this); //TODO: Add Playlist as second parameter once I have that class set upTrackDisplayID, displayPosition, this);
                        Debug.Log("Context menu initialized");

                        // Optional: Prevent other click handlers from firing once the menu is created
                        //eventData.Use();  // Stop propagation if context menu is opened

                    }
                }
            }
            else
            {
                //listDisplayManager.OnTrackSelection(this.gameObject);
                listDisplayManager.HandleTrackButtonClick(this, null);
            }
        }
        //This method exists to be called from the UITrackListDisplayManager,
        //so when the track is removed it destroys the Track Displays Context Menu if it is already open
        public void DestroyContextMenu()
        {
            
            Debug.Log("Delegating context menu destruction");
            if (ContextMenuInstance != null)
            {
                Debug.Log("All right now destroy it");
                ContextMenuInstance.DestroyContextMenu();
                ContextMenuInstance = null;
            }
        }

    }
}
