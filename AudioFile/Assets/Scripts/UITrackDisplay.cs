using UnityEngine;
using UnityEngine.UI;
using AudioFile.Model;
using TagLib.Riff;
using UnityEngine.EventSystems;

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
        public Button titleButton { get; private set; } 
        public Button artistButton { get; private set; } 
        public Button albumButton { get; private set; } 
        public Button durationButton { get; private set; }

        private UITrackListDisplayManager listDisplayManager;
        public GameObject ContextMenuPrefab; //Will need to drag this prefab into UI_Track_Display_Prefab
                                             //TODO: Add an AllPlaylist and AllPlaylistFolder collection (or something along those lines) reference
                                             //so whenever the user right clicks on a track, it populates the context menu with the appropriate options
                                             // these references here should just get from the global reference when called internally here

        private UIContextMenu contextMenuInstance;


        public string TrackDisplayID { get; private set; }

        private void Awake()
        {
            // Set TrackDisplayGameObject to the current GameObject this script is attached to
            TrackDisplayGameObject = this.gameObject;
        }
        public void Initialize(Track trackData, UITrackListDisplayManager manager)
        {
            listDisplayManager = manager;

            //Set TrackDisplayID to match TrackID so these can be compared later with LINQ queries
            TrackDisplayID = trackData.TrackProperties.GetProperty("TrackID");
            SetTrackData(trackData);
            InitializeButtons();
            //TrackDisplayGameObject.onClick.AddListener(() => OnPointerClick(PointerEventData eventData);

        }

        private void SetTrackData(Track trackData)
        {
            SetText(listDisplayManager.titleTextPath, trackData.TrackProperties.GetProperty("Title"));
            SetText(listDisplayManager.artistTextPath, trackData.TrackProperties.GetProperty("Artist"));
            SetText(listDisplayManager.albumTextPath, trackData.TrackProperties.GetProperty("Album"));
            SetText(listDisplayManager.durationTextPath, trackData.TrackProperties.GetProperty("Duration"));
            
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

        private void InitializeButtons()
        {
            titleButton = transform.Find("UI_Track_Title_Button").GetComponent<Button>();
            artistButton = transform.Find("UI_Track_Artist_Button").GetComponent<Button>();
            albumButton = transform.Find("UI_Track_Album_Button").GetComponent<Button>();
            durationButton = transform.Find("UI_Track_Duration_Button").GetComponent<Button>();

            /* Disable the duration button so it cannot be clicked. 
            Uncomment out later in case I want this functionality (or lack thereof)
            if (durationButton != null)
            {
                durationButton.interactable = false;
            }*/

            // Set up onClick events for Playback buttons
            if (titleButton != null) titleButton.onClick.AddListener(() => OnButtonClicked("Title"));
            if (artistButton != null) artistButton.onClick.AddListener(() => OnButtonClicked("Artist"));
            if (albumButton != null) albumButton.onClick.AddListener(() => OnButtonClicked("Album"));
            if (durationButton != null) durationButton.onClick.AddListener(() => OnButtonClicked("Duration"));
        }

        //This method exists to be called from the UITrackListDisplayManager,
        //so when the track is removed it destroys the Track Displays Context Menu if it is already open
        public void DestroyContextMenu() 
        {
            if (contextMenuInstance != null)
            {
                contextMenuInstance.DestroyContextMenu();
                contextMenuInstance = null;
            }
        }
        private void OnButtonClicked(string buttonType)
        {
            listDisplayManager.HandleTrackButtonClick(this, buttonType);
            Debug.Log("Button clicked: " + buttonType);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Pointer click detected");
            // Check for right-click
            if (eventData.button == PointerEventData.InputButton.Right)
            {

                Debug.Log($"Right-click detected on {transform.Find(UITrackListDisplayManager.Instance.titleTextPath).GetComponent<Text>()} - {transform.Find(UITrackListDisplayManager.Instance.titleTextPath).GetComponent<Text>()}");
                //UIContextMenu newContextMenuInstance = new UIContextMenu("Track Display Context Menu", TrackDisplayID);
                //GameObject contextMenuPrefab = (GameObject)Instantiate(Resources.Load("Track_Context_Menu"));
                //Transform contextMenuTransform = ContextMenuPrefab.GetComponent<RectTransform>();

                Canvas mainCanvas = FindObjectOfType<Canvas>();
                RectTransform canvasRectTransform = mainCanvas.GetComponent<RectTransform>();

                //GameObject contextMenuPrefab = Instantiate(ContextMenuPrefab, contextMenuTransform);


                if (ContextMenuPrefab != null && canvasRectTransform != null)
                {
                    GameObject newContextMenuInstance = Instantiate(ContextMenuPrefab, canvasRectTransform);
                    UIContextMenu contextMenu = newContextMenuInstance.GetComponent<UIContextMenu>();
                    if (contextMenu != null)
                    {
                        Debug.Log("Context menu initializing");
                        Vector2 displayPosition = eventData.position;
                        contextMenuInstance = contextMenu.Initialize(TrackDisplayID, displayPosition, this);
                        Debug.Log("Context menu initialized");

                    }
                    //contextMenuPrefab.InitializeContextMenu(contextMenuPrefab,TrackDisplayID);
                    //newContextMenuInstance.DisplayContextMenu(eventData.position);
                    Debug.Log("Somehow the context menu was null");
                }
                Debug.Log("Somehow the context menu prefab was null");
                // Create and display the context menu at the mouse position
                //DisplayContextMenu(eventData.position);
            }
            /*else if (eventData.button == PointerEventData.InputButton.Left)
            {
                Debug.Log($"Left_click detected. Exiting context menu.");
                //DestroyContextMenu(); //TODO: Implement this method
            }*/
        }
        /*private void InitializeContextMenu(GameObject menuInstance)
        {
            RemoveTrackCommand removeTrackCommand = new RemoveTrackCommand(UITrackListDisplayManager.Instance.GetTrackDisplayIndex(TrackDisplayGameObject)); //TODO: Change this and the RemoveTrackCommand class to take an ID as opposed to an index
            AddToPlaylistCommand addToPlaylistCommand = new AddToPlaylistCommand(TrackDisplayID); //TODO: Add Playlist as second parameter once I have that class set up

            Button addToPlaylistButton = menuInstance.transform.Find("Add_To_Playlist_Button").GetComponent<Button>();
            Button removeTrackButton = menuInstance.transform.Find("Remove_Button").GetComponent<Button>();

            Menu addToPlaylistMenu = new Menu(addToPlaylistButton, "Add to Playlist Menu");
            //TODO: Add a loop to add all Playlist Folders (sub menus) and Playlists names (Menu Items) to the addToPlaylistMenu
            MenuItem removeTrackMenuItem = new MenuItem(removeTrackButton, "Remove Track", removeTrackCommand);

        }

        private void DisplayContextMenu(Vector2 position)
        {
            Debug.Log("DisplayContextMenu() called");
            // Find the Canvas transform
            Transform canvasTransform = FindObjectOfType<Canvas>().transform;

            contextMenuInstance = Instantiate(ContextMenuPrefab, canvasTransform);

            // Set menu position to right-click location
            RectTransform menuRectTransform = contextMenuInstance.GetComponent<RectTransform>();
            menuRectTransform.position = position;

            InitializeContextMenu(contextMenuInstance);
        }*/


    }
}
