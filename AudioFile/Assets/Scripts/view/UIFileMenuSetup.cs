using AudioFile.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.Utilities;


namespace AudioFile.View
{
    /// <summary>
    /// Concrete class called by the SetupController to Initialize the File Menu.
    /// <remarks>
    /// Members: Single member. Implements Initialize() from MonoBehaviour.
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// </summary>
    public class UIFileMenuSetup : MonoBehaviour
    {
        public void Initialize()
        {
            #region Commands
            //TrackLibraryController trackLibraryController = AudioFile.Controller.TrackLibraryController.Instance;
            AddTrackCommand addTrackCommand = new AddTrackCommand();
            AddPlaylistCommand addPlaylistCommand = new AddPlaylistCommand();
            AddPlaylistFolderCommand addPlaylistFolderCommand = new AddPlaylistFolderCommand();
            ExitProgramCommand exitProgramCommand = new ExitProgramCommand();
            // TODO: Add more commands

            #endregion

            #region GameObject containers
            GameObject fileGameObject = GameObject.Find("File_Button");
            GameObject addGameObject = GameObject.Find("Add_Button");
            GameObject newGameObject = GameObject.Find("New_Button");
            GameObject newPlaylistGameObject = GameObject.Find("New_Playlist_Button");
            GameObject newPlaylistFolderGameObject = GameObject.Find("New_Playlist_Folder_Button");
            GameObject exitGameObject = GameObject.Find("Exit_Button");
            #endregion

            #region Buttons
            Button fileButton = fileGameObject.GetComponent<Button>();
            Button addButton = addGameObject.GetComponent<Button>();
            Button newButton = newGameObject.GetComponent<Button>();
            Button newPlaylistButton = newPlaylistGameObject.GetComponent<Button>();
            Button newPlaylistFolderButton = newPlaylistFolderGameObject.GetComponent<Button>();
            Button exitButton = exitGameObject.GetComponent<Button>();
            #endregion

            #region Menu Components
            Menu fileMenu = fileGameObject.AddComponent<Menu>();
            fileMenu = fileMenu.Initialize(fileButton, "File", "File Menu", true);

            MenuItem addTrackMenuItem = addGameObject.AddComponent<MenuItem>();
            addTrackMenuItem = addTrackMenuItem.Initialize(addButton, "Add", "Add New Track", addTrackCommand);

            Menu newMenu = newGameObject.AddComponent<Menu>();
            newMenu = newMenu.Initialize(newButton, "New...", "New Item Menu");

            MenuItem newPlaylistMenuItem = newPlaylistGameObject.AddComponent<MenuItem>();
            newPlaylistMenuItem = newPlaylistMenuItem.Initialize(newPlaylistButton, "Playlist", "Add empty playlist to library", addPlaylistCommand);

            MenuItem newPlaylistFolderMenuItem = newPlaylistFolderGameObject.AddComponent<MenuItem>();
            newPlaylistFolderMenuItem = newPlaylistFolderMenuItem.Initialize(newPlaylistFolderButton, "Playlist Folder", "Add empty playlist folder to library", addPlaylistFolderCommand);

            MenuItem exitMenuItem = exitGameObject.AddComponent<MenuItem>();
            exitMenuItem = exitMenuItem.Initialize(exitButton, "Exit", "Exit program", exitProgramCommand);
            #endregion

            #region Menu Set-up
            fileMenu.Add(addTrackMenuItem);
            fileMenu.Add(newMenu);
            fileMenu.Add(exitMenuItem);

            newMenu.Add(newPlaylistMenuItem);
            newMenu.Add(newPlaylistFolderMenuItem);

            //Set initial active state of menu items/submenus
            addButton.gameObject.SetActive(false);
            newButton.gameObject.SetActive(false);
            newPlaylistButton.gameObject.SetActive(false);
            newPlaylistFolderButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(false);
            #endregion
        }
    }
}
