using AudioFile.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


namespace AudioFile.View
{
    public class FileMenuSetup : MonoBehaviour
    {
        public void Initialize()
        {
            #region Commands
            TrackLibraryController trackLibraryController = AudioFile.Controller.TrackLibraryController.Instance;
            AddTrackCommand addTrackCommand = new AddTrackCommand(trackLibraryController);
            ExitProgramCommand exitProgramCommand = new ExitProgramCommand();
            AddPlaylistCommand addPlaylistCommand = new AddPlaylistCommand();
            AddPlaylistFolderCommand addPlaylistFolderCommand = new AddPlaylistFolderCommand();
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
            Menu fileMenu = new Menu(fileButton, "File Menu", true);
            MenuItem addTrackMenuItem = new MenuItem(addButton, "Add track to library", addTrackCommand);
            Menu newMenu = new Menu(newButton, "New Item Menu");
            MenuItem newPlaylistMenuItem = new MenuItem(newPlaylistButton, "Add empty playlist to library", addPlaylistCommand);
            MenuItem newPlaylistFolderMenuItem = new MenuItem(newPlaylistFolderButton, "Add empty playlist folder to library", addPlaylistFolderCommand);
            MenuItem exitMenuItem = new MenuItem(exitButton, "Exit program", exitProgramCommand);

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
