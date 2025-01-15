using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AudioFile.View
{
    public class UIToggleDisplay : MonoBehaviour
    {
        public Slider Display_Slider; //Drag this here in the inspector
        //readonly string toggleDisplayButtonPath = "Display_Toggle_Button";
        public bool IsTrackDisplayActive = true;

        public Button toggleDisplayButton;
        private void Start()
        {
            toggleDisplayButton.onClick.AddListener(ToggleDisplay);
        }

        public void ToggleDisplay()
        {
            //Toggles boolean flag
            IsTrackDisplayActive = !IsTrackDisplayActive;

            //Gets the track list display Game Object
            var trackDisplay = UITrackListDisplayManager.Instance.gameObject;
            var sortButtons = UISortButtonsManager.Instance.gameObject;

            //Add code to turn on the Visualizer Display when Track Display is turned off
            if (IsTrackDisplayActive)
            {
                Debug.Log("Track List display active");
                this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Hide Tracks";
                trackDisplay.SetActive(true);
                sortButtons.SetActive(true);
                Display_Slider.gameObject.SetActive(true);
                Display_Slider.value = 1f;
            }
            else
            {
                Debug.Log("Track List display in-active");
                this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Show Tracks";
                //Display_Slider.value = 0.2f;
                trackDisplay.SetActive(false);
                sortButtons.SetActive(false);
                Display_Slider.gameObject.SetActive(false);
            }
        }
        void OnDestroy()
        {
            // Remove the listener to avoid memory leaks
            if (toggleDisplayButton != null)
            {
                toggleDisplayButton.onClick.RemoveListener(ToggleDisplay);
            }
        }

    }
}
