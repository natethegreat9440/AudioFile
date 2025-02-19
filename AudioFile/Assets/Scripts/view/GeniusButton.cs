using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AudioFile.View
{
    /// <summary>
    /// View class that extends additional properties to the Unity Button class used by the UIGeniusButtonManager class. Class file includes the GeniusButtonState enum.
    /// <remarks>
    /// Members: State of type GeniusButtonState
    /// </remarks>
    /// <see cref="UnityEngine.UI.Button"/>
    /// <see also cref="UIGeniusButtonManager"/>
    /// </summary>
    public enum GeniusButtonState { Default, Searching, NotFound, Found }
    public class GeniusButton : Button
    {
        public GeniusButtonState State { get; set; }
    }
}
