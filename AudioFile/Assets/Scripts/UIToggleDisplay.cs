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
        readonly string toggleDisplayButtonPath = "Display_Toggle_Button";
        bool isTrackDisplayActive = true;
        private void Start()
        {
            Button toggleDisplayButton = GameObject.Find(toggleDisplayButtonPath).GetComponent<Button>();
            toggleDisplayButton.onClick.AddListener(ToggleDisplay);
        }

        private void ToggleDisplay()
        {
            //Toggles boolean flag
            isTrackDisplayActive = !isTrackDisplayActive;

            //Gets the track list display Game Object
            var trackDisplay = UITrackListDisplayManager.Instance.gameObject;

            //Add code to turn on the Visualizer Display when Track Display is turned off
            if (isTrackDisplayActive)
            {
                Debug.Log("Track List display active");
                this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Show Visual";
                trackDisplay.SetActive(true);
            }
            else
            {
                Debug.Log("Track List display active");
                this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Show Tracks";
                trackDisplay.SetActive(false);
            }
        }
    }
}
