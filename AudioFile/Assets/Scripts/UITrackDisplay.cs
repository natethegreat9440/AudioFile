using UnityEngine;
using UnityEngine.UI;
using AudioFile.Model;
using TagLib.Riff;

namespace AudioFile.View
{
    /// <summary>
    /// Concrete class for a UI Track Display item which holds Title, Artist, Album, and Duration buttons.
    /// <remarks>
    /// This needs to be attached as a component to the UI_Track_Display_Prefab
    /// Members: Initialize(), SetTrackData(), SetText(), InitializeButtons(), OnButtonClicked().
    /// OnButtonClicked() calls HandleTrackButtonClick() from the UITrackListDisplayManager.
    /// Implements Awake() from MonoBehaviour.
    /// </remarks>
    /// <see cref="UITrackListDisplayManager"/>
    /// <seealso cref="MonoBehaviour"/>
    /// </summary>

    public class UITrackDisplay : MonoBehaviour
    {
        public GameObject TrackDisplayGameObject { get; private set; }
        private Button titleButton, artistButton, albumButton, durationButton;
        private UITrackListDisplayManager listDisplayManager;

        public string TrackDisplayID { get; private set; }

        private void Awake()
        {
            // Set TrackDisplayGameObject to the current GameObject this script is attached to
            TrackDisplayGameObject = this.gameObject;
        }
        public void Initialize(Track trackData, UITrackListDisplayManager manager)
        {
            listDisplayManager = manager;

            //Set TrackDisplayID to match TrackID so these can be compared later with LINQ queries
            TrackDisplayID = trackData.TrackProperties.GetProperty("TrackID");
            SetTrackData(trackData);
            InitializeButtons();
        }

        private void SetTrackData(Track trackData)
        {
            SetText(listDisplayManager.titleTextPath, trackData.TrackProperties.GetProperty("Title"));
            SetText(listDisplayManager.artistTextPath, trackData.TrackProperties.GetProperty("Artist"));
            SetText(listDisplayManager.albumTextPath, trackData.TrackProperties.GetProperty("Album"));
            SetText(listDisplayManager.durationTextPath, trackData.TrackProperties.GetProperty("Duration"));
        }

        private void SetText(string path, string value)
        {
            var textTransform = transform.Find(path);
            if (textTransform != null)
            {
                var textComponent = textTransform.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = "  " + value; // Adding some space for aesthetic reasons.
                }
            }
        }

        private void InitializeButtons()
        {
            titleButton = transform.Find("UI_Track_Title_Button").GetComponent<Button>();
            artistButton = transform.Find("UI_Track_Artist_Button").GetComponent<Button>();
            albumButton = transform.Find("UI_Track_Album_Button").GetComponent<Button>();
            durationButton = transform.Find("UI_Track_Duration_Button").GetComponent<Button>();

            /* Disable the duration button so it cannot be clicked. 
            Uncomment out later in case I want this functionality (or lack thereof)
            if (durationButton != null)
            {
                durationButton.interactable = false;
            }*/

            // Set up onClick events for Playback buttons
            if (titleButton != null) titleButton.onClick.AddListener(() => OnButtonClicked("Title"));
            if (artistButton != null) artistButton.onClick.AddListener(() => OnButtonClicked("Artist"));
            if (albumButton != null) albumButton.onClick.AddListener(() => OnButtonClicked("Album"));
            if (durationButton != null) durationButton.onClick.AddListener(() => OnButtonClicked("Duration"));
        }

        private void OnButtonClicked(string buttonType)
        {
            listDisplayManager.HandleTrackButtonClick(this, buttonType);
            Debug.Log("Button clicked: " + buttonType);
        }
    }
}
