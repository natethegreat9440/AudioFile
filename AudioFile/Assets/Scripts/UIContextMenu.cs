using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AudioFile.View
{
    public class UIContextMenu :  MonoBehaviour, IPointerClickHandler
    {

        //public GameObject ContextMenuPrefab; //Will need to drag this prefab into UI_Track_Display_Prefab
                                             //TODO: Add an AllPlaylist and AllPlaylistFolder collection (or something along those lines) reference
                                             //so whenever the user right clicks on a track, it populates the context menu with the appropriate options
                                             // these references here should just get from the global reference when called internally here

        public GameObject ContextMenuGameObject;

        private string TrackDisplayID;
        private UITrackDisplay trackDisplay;

        private List<MenuComponent> menuComponents;

        /*public UIContextMenu(string description, string trackDisplayID) 
        {
            TrackDisplayID = trackDisplayID;
            Transform prefabTransform = ContextMenuPrefab.GetComponent<RectTransform>();
            contextMenuInstance = MonoBehaviour.Instantiate(ContextMenuPrefab, prefabTransform);
            InitializeContextMenu(contextMenuInstance);
        }*/
        private void Awake()
        {
            // Set TrackDisplayGameObject to the current GameObject this script is attached to
            ContextMenuGameObject = this.gameObject;
        }
        public UIContextMenu Initialize(string trackDisplayID, Vector2 position, UITrackDisplay manager)
        {
            trackDisplay = manager;
            TrackDisplayID = trackDisplayID;
            //Transform prefabTransform = contextMenuPrefab.GetComponent<RectTransform>();
            //contextMenuInstance = MonoBehaviour.Instantiate((GameObject)contextMenuPrefab, prefabTransform);

            RemoveTrackCommand removeTrackCommand = new RemoveTrackCommand(TrackDisplayID); 
            AddToPlaylistCommand addToPlaylistCommand = new AddToPlaylistCommand(TrackDisplayID); //TODO: Add Playlist as second parameter once I have that class set up

            Button addToPlaylistButton = ContextMenuGameObject.transform.Find("Add_To_Playlist_Button").GetComponent<Button>();
            Button removeTrackButton = ContextMenuGameObject.transform.Find("Remove_Button").GetComponent<Button>();

            Menu addToPlaylistMenu = new Menu(addToPlaylistButton, "Add to Playlist Menu");
            //TODO: Add a loop to add all Playlist Folders (sub menus) and Playlists names (Menu Items) to the addToPlaylistMenu
            MenuItem removeTrackMenuItem = new MenuItem(removeTrackButton, "Remove Track", removeTrackCommand);

            menuComponents = new List<MenuComponent>();
            menuComponents.Add(addToPlaylistMenu);
            menuComponents.Add(removeTrackMenuItem);

            // Set menu position to right-click location
            RectTransform menuRectTransform = ContextMenuGameObject.GetComponent<RectTransform>();
            menuRectTransform.position = position;

            return this;
        }

        public void OnPointerClick(PointerEventData eventData) //Needs to destroy itself if it is clicked outside of its own transform area
        {
            // Check if the click is outside the context menu or any of its children
            bool clickOutside = true;

            // Iterate through all child UI elements of the context menu
            foreach (Transform child in ContextMenuGameObject.transform)
            {
                RectTransform rectTransform = child.GetComponent<RectTransform>();
                if (rectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, eventData.pressEventCamera))
                {
                    clickOutside = false;
                    break;
                }
            }

            // Destroy the context menu if the click is outside
            if (clickOutside)
            {
                trackDisplay.DestroyContextMenu();
            }
        }
        /*public void DisplayContextMenu(Vector2 position)
        {
            Debug.Log("DisplayContextMenu() called");
            // Find the Canvas transform
            Transform canvasTransform = MonoBehaviour.FindObjectOfType<Canvas>().transform;

            //contextMenuInstance = MonoBehaviour.Instantiate(ContextMenuPrefab, canvasTransform);

            // Set menu position to right-click location
            RectTransform menuRectTransform = contextMenuInstance.GetComponent<RectTransform>();
            menuRectTransform.position = position;

            //InitializeContextMenu(contextMenuInstance);
        }*/

        public void DestroyContextMenu()
        {
            if (ContextMenuGameObject != null)
            {
                // Destroy the context menu instance and all its children
                Destroy(ContextMenuGameObject);
                ContextMenuGameObject = null;

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
