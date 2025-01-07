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
    public class UISortButtonsManager : MonoBehaviour
    {
        private static readonly Lazy<UISortButtonsManager> _instance = new Lazy<UISortButtonsManager>(CreateSingleton);

        // Private constructor to prevent instantiation
        private UISortButtonsManager() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static UISortButtonsManager Instance => _instance.Value;

        private static UISortButtonsManager CreateSingleton()
        {
            // Check if an instance already exists in the scene
            UISortButtonsManager existingInstance = FindObjectOfType<UISortButtonsManager>();
            if (existingInstance != null)
            {
                return existingInstance;
            }

            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(UITrackListDisplayManager).Name);
            // Add the UITrackListDisplayManager component to the GameObject
            UISortButtonsManager instance = singletonObject.AddComponent<UISortButtonsManager>();

            // Ensure the GameObject is not destroyed on scene load
            DontDestroyOnLoad(singletonObject);

            return instance;
        }
    }
}
