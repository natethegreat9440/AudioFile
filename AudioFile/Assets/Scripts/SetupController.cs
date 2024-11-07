using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioFile;
using AudioFile.Model;
using AudioFile.View;
using AudioFile.ObserverManager;
using System;
using System.Windows.Forms;

namespace AudioFile.Controller
{
    public class SetupController : MonoBehaviour, IController
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<SetupController> _instance = new Lazy<SetupController>(() => new SetupController());

        // Private constructor to prevent instantiation
        private SetupController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static SetupController Instance => _instance.Value;

        private UIFileMenuSetup fileMenuSetup;

        //private TrackLibrary trackLibrary;
        //No CreateSingleton() method for this controller as it is already attached to a GameObject in the scene

        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        public void Start()
        {
            SetupFileMenu(); //This script also sets up TrackLibraryController
            //TrackLibrary trackLibrary = AudioFile.Model.TrackLibrary.Instance;

        }
        public void Initialize()
        {
            throw new NotImplementedException();
        }
        public void HandleRequest(object request, bool isUndo)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private void SetupFileMenu()
        {
            GameObject fileMenuSetupObject = new GameObject("FileMenuSetup");
            fileMenuSetup = fileMenuSetupObject.AddComponent<UIFileMenuSetup>();
            fileMenuSetup.Initialize();
        }

        /*private void SetupLibrary()
        {
            GameObject trackLibraryObject = new GameObject("TrackLibraryObject");
            trackLibrary = trackLibraryObject.AddComponent<TrackLibrary>();
            trackLibrary.Initialize();
        }*/
    }
}
