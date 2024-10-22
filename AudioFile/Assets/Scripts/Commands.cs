using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

namespace AudioFile
{
    public interface ICommand
    {
        void Execute();
    }

    public class NewTrackCommand : ICommand
    {
        /* TODO: Implement this later once Controller is implemented
        private IController controller;
        public NewCommand(IController controller)
        {
            this.controller = controller;
        }
        */
        public void Execute()
        {
            //controller.AddTrack();
            Debug.Log("New Track Command executed");
        }
    }
}
