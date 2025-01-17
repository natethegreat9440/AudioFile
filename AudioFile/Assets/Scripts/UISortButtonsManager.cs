﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.Controller;
using AudioFile.Model;
using AudioFile.ObserverManager;
using AudioFile.Utilities;
using Unity.VisualScripting;

namespace AudioFile.View
{
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


        //TODO:
        //Use LINQ or collection specific methods to sort the library
        //Need reference (or get method) to find all track displays in TrackListDisplayerManager
        //Need a sort alphabetically method and a reverse sort method (delegates to a SortController that takes a list, a sorting property, and an algorithm and then applies a sorting method to the list)
        //When library gets sorted send an update to ListDisplayManager to refresh the display
        //Need a way to keep track of default order tracks were added in
        //Need a sort numerically method and a reverse sort method (unless alphabetical method can already handle this)
        //Need a way to update the text in each butons to include a an up arrow char or a down arrow char to show the direction of the sort

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

            //Set up onClick Listeners/events for SortButtons
            foreach (var button in SortButtons)
            {
                ConfigureSortButton(button);
            }

        }

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
                        SortController.Instance.HandleRequest(new SortCommand(button.State, TracksDisplayed, button.SortProperty), false);
                    }
                    else if (button.State == SortButtonState.Forward)
                    {
                        button.State = SortButtonState.Reverse;
                        SortController.Instance.HandleRequest(new SortCommand(button.State, TracksDisplayed, button.SortProperty), false);
                    }
                    else if (button.State == SortButtonState.Reverse)
                    {
                        button.State = SortButtonState.Default;
                        SortController.Instance.HandleRequest(new SortCommand(button.State, TracksDisplayed, button.SortProperty), false);
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
