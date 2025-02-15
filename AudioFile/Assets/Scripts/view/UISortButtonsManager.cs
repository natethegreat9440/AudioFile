using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.Controller;
using AudioFile.Model;
using AudioFile.Utilities;
using Unity.VisualScripting;

namespace AudioFile.View
{
    /// <summary>
    /// View class for managing the Sort Button behaviour and interactions.
    /// <remarks>
    /// Members: SortButtons, ConfigureSortButton(), SetAllButtonText(), EnableButtons(), DisableButtons(), . Implements Start(), Update(), Initialize() from MonoBehaviour.
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// </summary>

    public class UISortButtonsManager : MonoBehaviour
    {
        private static readonly Lazy<UISortButtonsManager> _instance = new Lazy<UISortButtonsManager>(CreateSingleton);

        // Private constructor to prevent instantiation
        private UISortButtonsManager() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static UISortButtonsManager Instance => _instance.Value;

        private static UISortButtonsManager CreateSingleton()
        {
            // Check if an instance already exists in the scene
            UISortButtonsManager existingInstance = FindObjectOfType<UISortButtonsManager>();
            if (existingInstance != null)
            {
                return existingInstance;
            }

            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(UITrackListDisplayManager).Name);
            // Add the UITrackListDisplayManager component to the GameObject
            UISortButtonsManager instance = singletonObject.AddComponent<UISortButtonsManager>();

            // Ensure the GameObject is not destroyed on scene load
            DontDestroyOnLoad(singletonObject);

            return instance;
        }

        readonly string titleSortButtonPath = "Sort_Buttons/Sort_Title_Button";
        readonly string artistSortButtonPath = "Sort_Buttons/Sort_Artist_Button";
        readonly string albumSortButtonPath = "Sort_Buttons/Sort_Album_Button";
        readonly string durationSortButtonPath = "Sort_Buttons/Sort_Duration_Button";

        SortButton titleSortButton;
        SortButton artistSortButton;
        SortButton albumSortButton;
        SortButton durationSortButton;

        string TracksDisplayed => UITrackListDisplayManager.Instance.TracksDisplayed;

        public List<SortButton> SortButtons;

        private void Start()
        {

            Canvas canvas = GameObject.Find("GUI_Canvas").GetComponent<Canvas>();

            //Set up Button GameObjects
            titleSortButton = canvas.transform.Find(titleSortButtonPath).GetComponent<SortButton>();
            artistSortButton = canvas.transform.Find(artistSortButtonPath).GetComponent<SortButton>();
            albumSortButton = canvas.transform.Find(albumSortButtonPath).GetComponent<SortButton>();
            durationSortButton = canvas.transform.Find(durationSortButtonPath).GetComponent<SortButton>();

            //Assign SortProperties to SortButtons
            titleSortButton.SortProperty = "Title";
            artistSortButton.SortProperty = "Artist";
            albumSortButton.SortProperty = "Album";
            durationSortButton.SortProperty = "Duration";

            // Initialize the SortButtons list after the SortButtons are assigned
            SortButtons = new List<SortButton>()
            {
                titleSortButton,
                artistSortButton,
                albumSortButton,
                durationSortButton,
            };

            foreach (var button in SortButtons)
            {
                ConfigureSortButton(button);
            }

        }

        void Update()
        {
            if (UITrackListDisplayManager.Instance.SelectedTrackDisplays.Count > 1)
            {
                DisableButtons();
            }
            else
            {
                EnableButtons();
            }
        }

        //Sets up onClick Listeners/events for SortButtons in ConfigureSortButton()
        private void ConfigureSortButton(SortButton button)
        {
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    Debug.Log($"{button.SortProperty} sort button clicked.");

                    if (button.State == SortButtonState.Default)
                    {
                        button.State = SortButtonState.Forward;
                        SortController.Instance.HandleUserRequest(new SortCommand(button.State, TracksDisplayed, button.SortProperty), false);
                    }
                    else if (button.State == SortButtonState.Forward)
                    {
                        button.State = SortButtonState.Reverse;
                        SortController.Instance.HandleUserRequest(new SortCommand(button.State, TracksDisplayed, button.SortProperty), false);
                    }
                    else if (button.State == SortButtonState.Reverse)
                    {
                        button.State = SortButtonState.Default;
                        SortController.Instance.HandleUserRequest(new SortCommand(button.State, TracksDisplayed, button.SortProperty), false);
                    }

                    Debug.Log($"Sort method is {button.State}");

                    SetAllButtonText(button);

                });
            }
        }

        private static void SetAllButtonText(SortButton passedButton)
        {
            switch (passedButton.State)
            {
                case SortButtonState.Forward:
                    passedButton.GetComponentInChildren<Text>().text = passedButton.SortProperty == "Duration" ? "Time ↑" : $"{passedButton.SortProperty} ↑";
                    break;
                case SortButtonState.Reverse:
                    passedButton.GetComponentInChildren<Text>().text = passedButton.SortProperty == "Duration" ? "Time ↓" : $"{passedButton.SortProperty} ↓";
                    break;
                case SortButtonState.Default:
                    passedButton.GetComponentInChildren<Text>().text = passedButton.SortProperty == "Duration" ? "Time" : $"{passedButton.SortProperty}";
                    break;
            }

            foreach (var button in Instance.SortButtons) //Sets all other SortButtons state and text to default
            {
                if (button != passedButton)
                {
                    button.State = SortButtonState.Default;
                    button.GetComponentInChildren<Text>().text = button.SortProperty == "Duration" ? "Time" : $"{button.SortProperty}";
                }
            }
        }

        private void EnableButtons()
        {
            foreach (var button in Instance.SortButtons)
            {
                button.interactable = true;
            }
        }

        private void DisableButtons()
        {
            foreach (var button in Instance.SortButtons)
            {
                button.interactable = false;
            }
        }
    }
}
