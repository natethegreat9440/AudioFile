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
    /// Members: TrackDisplayID, IsSelected, Initialize(), SetTrackDisplayText(), SetButtonText(), GetTrackDisplayText(), InitializeButtons(), OnButtonClicked(), DestroyContextMenu().
    /// OnButtonClicked() calls HandleTrackButtonClick() from the UITrackListDisplayManager.
    /// Implements Awake() from MonoBehaviour. Implements OnPointerClick() from IPointerClickHandler from UnityEngine.EventSystems
    /// </remarks>
    /// <see cref="UITrackListDisplayManager"/>
    /// <seealso cref="MonoBehaviour"/>
    /// <see cref="IPointerClickHandler"/>
    /// </summary>

    public class UITrackDisplay : MonoBehaviour, IPointerClickHandler
    {
        [HideInInspector]
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
            SetTrackDisplayText(trackData);
            InitializeButtons();

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
                    HandleTrackDisplayContextMenuCreation(eventData, canvasRectTransform);
                }
            }
            else //Delegate to UITrackListDisplayManager
            {
                listDisplayManager.HandleTrackButtonClick(this, null);
            }
        }

        private void HandleTrackDisplayContextMenuCreation(PointerEventData eventData, RectTransform canvasRectTransform)
        {
            GameObject newContextMenuInstance = Instantiate(ContextMenuPrefab, canvasRectTransform);
            UIContextMenu contextMenu = newContextMenuInstance.GetComponent<UIContextMenu>();
            if (contextMenu != null)
            {
                Vector2 displayPosition = eventData.position;

                var trackDisplayIDList = new List<int>();

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

        public void DestroyContextMenu() //This method exists to be called from the UITrackListDisplayManager, so when the track is removed it destroys the Track Displays Context Menu if it is already open

        {

            Debug.Log("Delegating context menu destruction");
            if (ContextMenuInstance != null)
            {
                Debug.Log("All right now destroy it");
                ContextMenuInstance.DestroyContextMenu();
                ContextMenuInstance = null;
            }
        }
        private void OnButtonClicked(string buttonType)
        {
            listDisplayManager.HandleTrackButtonClick(this, buttonType);
            Debug.Log("Button clicked: " + buttonType);
        }

        public string GetTrackDisplayText(string path)
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

        private void SetTrackDisplayText(Track track)
        {
            //For Testing and easily checking TrackID and AlbumTrackNumber which aren't intended to be displayed to the user
            //string title = (string)track.TrackProperties.GetProperty(track.TrackID, "Title") + " " + track.TrackID + " " + track.TrackProperties.GetProperty(track.TrackID, "AlbumTrackNumber");
            //SetButtonText(listDisplayManager.titleTextPath, title);

            SetButtonText(listDisplayManager.titleTextPath, (string)track.TrackProperties.GetProperty(track.TrackID, "Title"));
            SetButtonText(listDisplayManager.artistTextPath, (string)track.TrackProperties.GetProperty(track.TrackID, "Artist"));
            SetButtonText(listDisplayManager.albumTextPath, (string)track.TrackProperties.GetProperty(track.TrackID, "Album"));
            SetButtonText(listDisplayManager.durationTextPath, (string)track.TrackProperties.GetProperty(track.TrackID, "Duration"));
        }

        private void SetButtonText(string path, string value)
        {
            var textTransform = transform.Find(path);
            if (textTransform != null)
            {
                var textComponent = textTransform.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = "  " + value; // Adding some empty space for aesthetic purposes.
                }
            }
        }
    }
}
