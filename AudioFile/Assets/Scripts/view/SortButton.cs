using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.Controller;
using AudioFile.Model;
using AudioFile.ObserverManager;
using AudioFile.Utilities;
using Unity.VisualScripting;

namespace AudioFile.View
{
    public enum SortButtonState { Default, Forward, Reverse, }

    public class SortButton : Button
    {
        public SortButtonState State { get; set; }

        public string SortProperty { get; set; }
    }
}
