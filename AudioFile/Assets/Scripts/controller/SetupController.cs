using System.Collections;
using System.Collections.Generic;
using AudioFile;
using AudioFile.Model;
using System.Windows.Forms;
using AudioFile.ObserverManager;
using UnityEngine;
using AudioFile.View;
using System;
using Mono.Data.Sqlite;
using System.IO;

namespace AudioFile.Controller
{
    /// <summary>
    /// Generic singleton controller for setting up the UI where needed (i.e., Menus)
    /// <remarks>
    /// This should be attached to a GameObject in the scene for the easiest entry point for setting up the UI
    /// Members: SetupFileMenu(). Implements Start() and Awake() from MonoBehaviour. 
    /// This controller has no implementation for IController methods (yet).
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IController"/>
    /// </summary>

    public class SetupController : MonoBehaviour, IController
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        //private static readonly Lazy<SetupController> _instance = new Lazy<SetupController>(CreateSingleton);

        //// Private constructor to prevent instantiation
        //private SetupController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        //public static SetupController Instance => _instance.Value;

        //private static SetupController CreateSingleton()
        //{
        //    // Create a new GameObject to hold the singleton instance if it doesn't already exist
        //    GameObject singletonObject = new GameObject(typeof(SetupController).Name);

        //    // Add the PlayBackController component to the GameObject
        //    return singletonObject.AddComponent<SetupController>();
        //}

        private static SetupController _instance;
        public static SetupController Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("SetupController is not initialized in the scene.");
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }


        private UIFileMenuSetup fileMenuSetup;

        //No CreateSingleton() method needed for this controller as it is already attached to a GameObject in the scene

        public string ConnectionString { get; private set; }

        public void Start()
        {
            ConnectionString = SetSQLLiteDBLocation();
            Debug.Log($"AudioFile.db located here {ConnectionString}");

            var newInstance = TrackLibraryController.Instance;
            var observerManager = ObserverManager.ObserverManager.Instance; //Only assigned to a reference variable here to wake it up for all of it's dependents
            var playbackController = PlaybackController.Instance;
            var trackLibrary = TrackLibrary.Instance;
            var trackListDisplayManager = UITrackListDisplayManager.Instance;
            var sortButtonManager = UISortButtonsManager.Instance;
            var sortController = SortController.Instance;
            var contextMenu = UIContextMenu.Instance;
            SetupFileMenu();
        }

        private static string SetSQLLiteDBLocation()
        {
            try
            {
                #if UNITY_EDITOR //This if statement only occurs while using the Unity Editor which defaults to the One Drive/MyDocuments folder for testing. Unfortunately Environment.SpecialFolder.MyDocuments defaults to the OneDrive/MyDocuments instead of the actual Documents folder on my PC. Maybe I'll fix that later        
                        string dbDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AudioFileDB");
                        if (!System.IO.Directory.Exists(dbDirectory))
                        {
                            System.IO.Directory.CreateDirectory(dbDirectory);
                        }
                        return "URI=file:" + dbDirectory.Replace("\\", "/") + "/AudioFile.db";
                #else
                        //return AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");; //Saves to same location as the executable, but this could result in permissions issues depending on if the default install location is selected on a non-admin user profile (ProgramFiles typically requires admin privileges when writing files)
                        return "URI=file:" + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("\\", "/") + "/AudioFile.db";
                #endif
            }
            catch (UnauthorizedAccessException)
            {
                Debug.LogError($"Error setting database file location. Write access denied.");
                ObserverManager.ObserverManager.Instance.NotifyObservers("AudioFileError", "Error writing tracks to memory. Write access denied. Please check with your system administrator.");
                return string.Empty;
            }
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
    }
}
