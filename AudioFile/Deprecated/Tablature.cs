using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tablature
{
    public string[] Lines { get; private set; }

    public Tablature(string[] lines)
    {
        Lines = lines;
    }

    public void Display()
    {
        foreach (var line in Lines)
        {
            Debug.Log(line);
        }
    }
}
