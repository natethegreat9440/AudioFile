﻿using UnityEngine;
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
}