using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.Controller;
using AudioFile.Model;
using AudioFile.Utilities;
using Unity.VisualScripting;

namespace AudioFile.View
{
    /// <summary>
    /// View class that extends additional properties to the Unity Button class used by the SortButtonManager class. Class file includes the SortButtonState enum.
    /// <remarks>
    /// Members: State of type SortButtonState, SortProperty.
    /// </remarks>
    /// <see cref="UnityEngine.UI.Button"/>
    /// <see also cref="UISortButtonsManager"/>
    /// </summary>
    public enum SortButtonState { Default, Forward, Reverse, }

    public class SortButton : Button
    {
        public SortButtonState State { get; set; }

        public string SortProperty { get; set; }
    }
}
