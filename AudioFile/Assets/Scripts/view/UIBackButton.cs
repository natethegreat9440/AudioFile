using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button; // Add this line to resolve ambiguity
using AudioFile.Controller;
using AudioFile.Model;
using AudioFile.Utilities;
using UnityEngine.Windows;


namespace AudioFile.View
{
    /// <summary>
    /// View class for the back button.
    /// <remarks>
    /// Members: 
    /// </remarks>
    /// <see cref="UnityEngine.UI.Button"/>
    /// </summary>
    public class UIBackButton : MonoBehaviour
    {
        readonly string backButtonPath = "Back_Button";
        bool isFiltered => SearchController.Instance.IsFiltered;

        Button backButton;

        private bool previousFilterState = true;
        void Start()
        {
            Canvas canvas = GameObject.Find("GUI_Canvas").GetComponent<Canvas>();

            backButton = canvas.transform.Find(backButtonPath).GetComponent<Button>();

            if (backButton != null)
            {
                backButton.onClick.AddListener(() => SearchController.Instance.HandleUserRequest(new BackCommand()));
            }

        }
        void Update()
        {
            if (isFiltered != previousFilterState)
            {
                previousFilterState = isFiltered;
                if (previousFilterState)
                {
                    EnableButton();
                }
                else
                {
                    DisableButton();
                }
            }
        }

        void EnableButton()
        {
            if (backButton != null)
            {
                backButton.interactable = true;
            }
        }
        void DisableButton()
        {
            if (backButton != null)
            {
                backButton.interactable = false;
            }
        }
    }
}
