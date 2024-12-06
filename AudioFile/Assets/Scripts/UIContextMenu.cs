using AudioFile.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AudioFile.Utilities;
using Unity.VisualScripting;

namespace AudioFile.View
{
    public class UIContextMenu : MonoBehaviour
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<UIContextMenu> _instance = new Lazy<UIContextMenu>(() => new GameObject("UIContextMenu").AddComponent<UIContextMenu>());

        // Private constructor to prevent instantiation
        private UIContextMenu() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static UIContextMenu Instance => _instance.Value;
        //TODO: Add an AllPlaylist and AllPlaylistFolder collection (or something along those lines) reference
        //so whenever the user right clicks on a track, it populates the context menu with the appropriate options
        // these references here should just get from the global reference when called internally here

        public GameObject ContextMenuGameObject;

        public Canvas mainCanvas;

        //private string TrackDisplayID;
        private List<string> TrackDisplayIDs;
        private UITrackDisplay trackDisplay;

        private List<MenuComponent> menuComponents;

        private ClickOutsideHandler ClickOutsideHandler;

        private void Awake()
        {
            // Set TrackDisplayGameObject to the current GameObject this script is attached to
            ContextMenuGameObject = this.gameObject;
        }
        public UIContextMenu Initialize(List<string> trackDisplayIDs, Vector2 position, UITrackDisplay manager)
        {
            trackDisplay = manager;
            TrackDisplayIDs = trackDisplayIDs;
            //ContextMenuGameObject = GameObject.Find("UIContextMenu");

            // Set menu position to right-click location
            RectTransform menuRectTransform = ContextMenuGameObject.GetComponent<RectTransform>();
            menuRectTransform.position = position;

            CreateClickCatcher();
            //Set as last sibling makes it so the context renders above the click catcher so the click 
            //catcher doesn't intercept mouse behavior
            ContextMenuGameObject.transform.SetAsLastSibling();
            InitializeMenu();

            UITrackListDisplayManager.Instance.activeContextMenu = this;
            return this;
        }

        private void InitializeMenu()
        {
            Debug.Log("Initializing context menu");
            GameObject contextMenu = ContextMenuGameObject;
            //GameObjectExtensions is a class I made to make finding game objects easier for this project. It is part of the Utilities namespace
            GameObject addToPlaylistGameObject = GameObjectExtensions.FindInChildren(contextMenu, "Add_To_Playlist_Button");
            GameObject removeTrackGameObject = GameObjectExtensions.FindInChildren(contextMenu, "Remove_Button");
            GameObject testPlaylistGameObject = GameObjectExtensions.FindInChildren(contextMenu, "Test_Playlist_Button");

            RemoveTrackCommand removeTrackCommand = new RemoveTrackCommand(TrackDisplayIDs);
            AddToPlaylistCommand addToPlaylistCommand = new AddToPlaylistCommand(TrackDisplayIDs); //TODO: Add Playlist as second parameter once I have that class set up

            Button addToPlaylistButton = ContextMenuGameObject.transform.Find("Add_To_Playlist_Button").GetComponent<Button>();
            Button removeTrackButton = ContextMenuGameObject.transform.Find("Remove_Button").GetComponent<Button>();
            Button testPlaylistButton = ContextMenuGameObject.transform.Find("Test_Playlist_Button").GetComponent<Button>();


            //Last argument in initialize is for a +1 correction factor to offset inconsistencies on how mouse position is tracked against OnPointerExit events
            //See comments in Menu code for more details. It is recommended for each Menu object added to the context menu to set optional parameter correctYMin to +1 to offset issues and get proper behavior
            Menu addToPlaylistMenu = addToPlaylistButton.AddComponent<Menu>();
            addToPlaylistMenu = addToPlaylistMenu.Initialize(addToPlaylistButton, "Add to Playlist...", "Add to Playlist Menu", false, 0f, 1f);

            //TODO: Add a loop to add all Playlist Folders (sub menus) and Playlists names (Menu Items) to the addToPlaylistMenu

            MenuItem removeTrackMenuItem = removeTrackGameObject.AddComponent<MenuItem>();
            removeTrackMenuItem = removeTrackMenuItem.Initialize(removeTrackButton, "Remove", "Remove Track", removeTrackCommand, true);

            //Added for testing submenu
            MenuItem testPlaylistMenuItem = testPlaylistGameObject.AddComponent<MenuItem>();
            testPlaylistMenuItem = testPlaylistMenuItem.Initialize(testPlaylistButton, "Playlist 1", "Dummy playlist for testing", addToPlaylistCommand, true);
            
            //Add submenus to menu items
            addToPlaylistMenu.Add(testPlaylistMenuItem);

            //Set initial active state of menu items/submenus
            testPlaylistButton.gameObject.SetActive(false);

            menuComponents = new List<MenuComponent>
            {
                addToPlaylistMenu,
                removeTrackMenuItem,
                testPlaylistMenuItem,
            };
        }

        //ClickCatcher is used to detect clicks outside the menu's transform
        private void CreateClickCatcher()
        {
            // Create the overlay for detecting clicks outside the menu
            mainCanvas = FindObjectOfType<Canvas>();
            GameObject overlay = new GameObject("ClickCatcherOverlay");
            overlay.transform.SetParent(mainCanvas.transform, false);
            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0); // Fully transparent
            overlayImage.raycastTarget = true;

            ClickOutsideHandler clickOutsideHandler = overlay.AddComponent<ClickOutsideHandler>();
            clickOutsideHandler.contextMenu = this;
            ClickOutsideHandler = clickOutsideHandler;
        }

        public void DestroyContextMenu()
        {
            Debug.Log("Entering DestroyContextMenu from UIContextMenu");
            if (ContextMenuGameObject != null)
            {
                Debug.Log("Destroying Context Menu from UIContextMenu");
                Destroy(ContextMenuGameObject);
                ContextMenuGameObject = null;
                ClickOutsideHandler.DestroyClickOutsideHandler();
                ClickOutsideHandler = null;
                // Clear the menu components list
                if (menuComponents != null)
                {
                    menuComponents.Clear();
                    menuComponents = null;
                }
            }
        }
    }
}
