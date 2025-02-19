using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Application = UnityEngine.Application;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button; // Add this line to resolve ambiguity
using AudioFile.Controller;
using System.Windows.Forms;
using Codice.CM.ConfigureHelper;

namespace AudioFile.View
{
    public class UIGeniusButtonManager : MonoBehaviour
    {
        private static readonly Lazy<UIGeniusButtonManager> _instance = new Lazy<UIGeniusButtonManager>(CreateSingleton);

        // Private constructor to prevent instantiation
        private UIGeniusButtonManager() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static UIGeniusButtonManager Instance => _instance.Value;

        private static UIGeniusButtonManager CreateSingleton()
        {
            // Check if an instance already exists in the scene
            UIGeniusButtonManager existingInstance = FindObjectOfType<UIGeniusButtonManager>();
            if (existingInstance != null)
            {
                return existingInstance;
            }

            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(UITrackListDisplayManager).Name);
            // Add the UITrackListDisplayManager component to the GameObject
            UIGeniusButtonManager instance = singletonObject.AddComponent<UIGeniusButtonManager>();

            // Ensure the GameObject is not destroyed on scene load
            DontDestroyOnLoad(singletonObject);

            return instance;
        }
        readonly string geniusButtonPath = "Launch_Genius_Button";

        public GeniusButton GeniusButton;

        public string targetUrl => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "GeniusUrl");

        string selectedTrackTitle => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "Title");

        string selectedTrackArtist => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "Artist");

        void Start()
        {
            Canvas canvas = GameObject.Find("GUI_Canvas").GetComponent<Canvas>();

            GeniusButton = GameObject.Find(geniusButtonPath).GetComponent<GeniusButton>();

            if (GeniusButton != null)
            {
                ConfigureGeniusButton();
            }
        }

        void Update()
        {
            ManageButtonText();

            if (GeniusButton != null && GeniusButton.State == GeniusButtonState.Found)
            {
                GeniusButton.interactable = true;
            }
            else
            {
                GeniusButton.interactable = false;
            }
        }

        private void ConfigureGeniusButton()
        {
            if (GeniusButton != null)
            {
                GeniusButton.onClick.AddListener(() => Application.OpenURL(targetUrl));
            }
        }

        private void ManageButtonText()
        {
            //Note leading spaces in parameters are intentional since I don't like how close the text starts to the left button border by default
            if (GeniusButton.State == GeniusButtonState.Default)
            {
                SetButtonText(" Select a track to activate Genius.com button.");
            }
            else if (GeniusButton.State == GeniusButtonState.Searching)
            {
                SetButtonText(" Searching Genius.com...");
            }
            else if (GeniusButton.State == GeniusButtonState.NotFound)
            {
                SetButtonText(" Could not find Genius.com page for selected track.");
            }
            else if (GeniusButton.State == GeniusButtonState.Found)
            {
                SetButtonText($" Click Genius.com button to visit track page for {selectedTrackTitle} by {selectedTrackArtist}");
            }
        }
        private void SetButtonText(string buttonText)
        {
            GeniusButton.GetComponentInChildren<Text>().text = buttonText;
        }
    }
}
