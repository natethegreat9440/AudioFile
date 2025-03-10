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
    public enum SampleDisplayTextState { Default, Searching, NotFound, Found }
    public class SampleDisplayText : TextMeshProUGUI
    {
        public SampleDisplayTextState State { get; set; }
    }

}
