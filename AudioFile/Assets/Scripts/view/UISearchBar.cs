using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioFile.Model;
using AudioFile.Controller;
using AudioFile.Utilities;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.View;

namespace AudioFile.View
{
    public class UISearchBar : InputField
    {
        public string ActiveCollection { get; set; } = "Tracks"; //TODO: Have this look to the currently selected MediaLibraryComponent collection managed by a MainDisplayController
        protected override void Start()
        {
            base.Start();

            // Listen for input submission (when Enter is pressed)
            //onEndEdit.AddListener(HandleSubmit);

            //Listens for input whenever a keystroke is pressed while input field is in focus
            onValueChanged.AddListener(HandleSubmit);
        }

        private void HandleSubmit(string input)
        {
            // Detect if Enter key was pressed
            //if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            //{
            //    Debug.Log("Search command sent");
            //    SearchController.Instance.HandleRequest(new SearchCommand(input, ActiveCollection));
            //}
            Debug.Log("Search command sent");
            SearchController.Instance.HandleRequest(new SearchCommand(input, ActiveCollection));
        }
    }
}
