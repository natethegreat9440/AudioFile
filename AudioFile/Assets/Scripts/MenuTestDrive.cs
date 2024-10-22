using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace AudioFile
{
    public class MenuTestDrive : MonoBehaviour
    {
        void Start()
        {
            #region Commands
            NewTrackCommand newTrackCommand = new NewTrackCommand();
            #endregion

            #region GameObjects
            GameObject fileGameObject = GameObject.Find("File Button");
            GameObject newGameObject = GameObject.Find("New Button");
            GameObject openGameObject = GameObject.Find("Open Button");
            GameObject optionsGameObject = GameObject.Find("Options Button");
            GameObject openChildGameObject = GameObject.Find("Open Child Button");
            GameObject option1GameObject = GameObject.Find("Option 1 Button");
            GameObject option2GameObject = GameObject.Find("Option 2 Button");
            GameObject option3GameObject = GameObject.Find("Option 3 Button");
            #endregion

            #region Buttons
            Button fileButton = fileGameObject.GetComponent<Button>();
            Button newButton = newGameObject.GetComponent<Button>();
            Button openButton = openGameObject.GetComponent<Button>();
            Button optionsButton = optionsGameObject.GetComponent<Button>();
            Button openChildButton = openChildGameObject.GetComponent<Button>();
            Button option1Button = option1GameObject.GetComponent<Button>();
            Button option2Button = option2GameObject.GetComponent<Button>();
            Button option3Button = option3GameObject.GetComponent<Button>();
            #endregion

            #region Menu Components
            Menu fileMenu = new Menu(fileButton, "File Menu");
            MenuItem newMenuItem = new MenuItem(newButton, "New Menu", newTrackCommand);
            Menu openMenu = new Menu(openButton, "Open Menu");
            Menu optionsMenu = new Menu(optionsButton, "Options Menu");
            Menu openChildMenu = new Menu(openChildButton, "Open Child Menu");
            Menu option1Menu = new Menu(option1Button, "Option 1 Menu");
            Menu option2Menu = new Menu(option2Button, "Option 2 Menu");
            Menu option3Menu = new Menu(option3Button, "Option 3 Menu");
            #endregion

            #region Menu Set-up
            fileMenu.Add(newMenuItem);
            fileMenu.Add(openMenu);
            fileMenu.Add(optionsMenu);

            openMenu.Add(openChildMenu);

            optionsMenu.Add(option1Menu);
            optionsMenu.Add(option2Menu);
            optionsMenu.Add(option3Menu);

            newButton.gameObject.SetActive(false);
            openButton.gameObject.SetActive(false);
            openChildButton.gameObject.SetActive(false);
            optionsButton.gameObject.SetActive(false);
            option1Button.gameObject.SetActive(false);
            option2Button.gameObject.SetActive(false);
            option3Button.gameObject.SetActive(false);
            #endregion
        }
    }
}
