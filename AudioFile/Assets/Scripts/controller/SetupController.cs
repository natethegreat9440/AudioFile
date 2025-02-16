using System.Collections;
using System.Collections.Generic;
using AudioFile;
using AudioFile.Model;
using System.Windows.Forms;
using AudioFile.Utilities;
using UnityEngine;
using AudioFile.View;
using System;
using Mono.Data.Sqlite;
using System.IO;
using Application = UnityEngine.Application;

namespace AudioFile.Controller
{
    /// <summary>
    /// Generic singleton controller for setting up the UI where needed (i.e., Menus)
    /// <remarks>
    /// This should be attached to a GameObject in the scene for the easiest entry point for setting up the UI
    /// Members: SetupFileMenu(), SetSQLLiteDBLocation(), . Implements Start() and Awake() from MonoBehaviour. 
    /// Holds the central reference to the main AudioFile.db file by means of ConnectionString.
    /// This controller has no implementation for IController methods (yet).
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IController"/>
    /// </summary>

    public class SetupController : MonoBehaviour, IController
    {
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

        private UIFileMenuSetup fileMenuSetup;

        //No CreateSingleton() method needed for this controller as it is already attached to a GameObject in the scene

        public string ConnectionString { get; private set; }

        public void Start()
        {
            ConnectionString = SetSQLLiteDBLocation();

            Debug.Log($"AudioFile.db located here {ConnectionString}");

            var observerManager = ObserverManager.Instance; //Only assigned to a reference variable here to wake it up for all of it's dependents
            var sortController = SortController.Instance;
            var searchController = SearchController.Instance;
            var trackLibraryController = TrackLibraryController.Instance;
            var trackSampleController = TrackSampleController.Instance;
            var playbackController = PlaybackController.Instance;
            var sortButtonManager = UISortButtonsManager.Instance;
            var contextMenu = UIContextMenu.Instance;
            var trackListDisplayManager = UITrackListDisplayManager.Instance;

            SetupFileMenu();
        }

        private void Awake()
        {
            SQLiteLoader.ForceLoadLocalSQLite(); //Manually loads the Mono.Data.Sqlite v1.0.61.0 assembly from Plugins rather than trying to find any other versions on the system
            Debug.Log($"Using SQLite version: {Mono.Data.Sqlite.SqliteConnection.SQLiteVersion}");
            Debug.Log($"Application.dataPath: {Application.dataPath}");

            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        private void SetupFileMenu()
        {
            GameObject fileMenuSetupObject = new GameObject("FileMenuSetup");
            fileMenuSetup = fileMenuSetupObject.AddComponent<UIFileMenuSetup>();
            fileMenuSetup.Initialize();
        }

        private static string SetSQLLiteDBLocation()
        {
            try
            {
                #if UNITY_EDITOR //This if statement only occurs while using the Unity Editor which defaults to the One Drive/MyDocuments folder for testing. Unfortunately Environment.SpecialFolder.MyDocuments defaults to the OneDrive/MyDocuments instead of the actual Documents folder on my PC. Maybe I'll fix that later        
                        return SetDirectoryForUnityEditor();
                #else
                        //return AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");; //Saves to same location as the executable, but this could result in permissions issues depending on if the default install location is selected on a non-admin user profile (ProgramFiles typically requires admin privileges when writing files)
                        return "URI=file:" + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("\\", "/") + "/AudioFile.db";
                #endif
            }
            catch (UnauthorizedAccessException)
            {
                Debug.LogError($"Error setting database file location. Write access denied.");
                ObserverManager.Instance.NotifyObservers("AudioFileError", "Error writing tracks to memory. Write access denied. Please check with your system administrator.");
                return string.Empty;
            }
        }

        private static string SetDirectoryForUnityEditor()
        {
            string dbDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AudioFileDB");
            if (!System.IO.Directory.Exists(dbDirectory))
            {
                System.IO.Directory.CreateDirectory(dbDirectory);
            }
            return "URI=file:" + dbDirectory.Replace("\\", "/") + "/AudioFile.db";
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
        public void HandleUserRequest(object request, bool isUndo)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
