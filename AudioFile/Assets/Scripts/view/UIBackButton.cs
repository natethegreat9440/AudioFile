using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button; // Add this line to resolve ambiguity
using AudioFile.Controller;
using AudioFile.Model;
using AudioFile.Utilities;
using Unity.VisualScripting;
using UnityEngine.UIElements;

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

        void Start()
        {
            Canvas canvas = GameObject.Find("GUI_Canvas").GetComponent<Canvas>();

            backButton = canvas.transform.Find(backButtonPath).GetComponent<Button>();

            if (backButton != null)
            {
                backButton.onClick.AddListener(() => SearchController.Instance.ClearSearch());
            }

        }
        void Update()
        {
            if (!isFiltered)
            {
                DisableButton();
            }
            else
            {
                EnableButton();
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
