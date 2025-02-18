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

namespace AudioFile.View
{
    public class UIGeniusButton : MonoBehaviour
    {
        readonly string geniusButtonPath = "Launch_Genius_Button";

        Button geniusButton;

        public string targetUrl => (string)PlaybackController.Instance.SelectedTrack.TrackProperties.GetProperty(PlaybackController.Instance.SelectedTrack.TrackID, "GeniusUrl");

        void Start()
        {
            Canvas canvas = GameObject.Find("GUI_Canvas").GetComponent<Canvas>();

            geniusButton = GameObject.Find(geniusButtonPath).GetComponent<Button>();

            if (geniusButton != null)
            {
                geniusButton.onClick.AddListener(() => Application.OpenURL(targetUrl));
            }
        }



    }
}
