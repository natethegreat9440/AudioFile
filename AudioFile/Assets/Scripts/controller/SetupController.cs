using System.Collections;
using System.Collections.Generic;
using AudioFile;
using AudioFile.Model;
using System.Windows.Forms;
using AudioFile.ObserverManager;
using UnityEngine;
using AudioFile.View;
using System;

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
        private static readonly Lazy<SetupController> _instance = new Lazy<SetupController>(() => new SetupController());

        // Private constructor to prevent instantiation
        private SetupController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static SetupController Instance => _instance.Value;

        private UIFileMenuSetup fileMenuSetup;

        //No CreateSingleton() method needed for this controller as it is already attached to a GameObject in the scene

        public string ConnectionString = "URI=file:" + UnityEngine.Application.persistentDataPath + "/AudioFile.db";
        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        public void Start()
        {
            var newInstance = TrackLibraryController.Instance;
            SetupFileMenu();
            var observerManager = ObserverManager.ObserverManager.Instance; //Only assigned to a reference variable here to wake it up for all of it's dependents
            var playbackController = PlaybackController.Instance;
            var trackLibrary = TrackLibrary.Instance;
            var trackListDisplayManager = UITrackListDisplayManager.Instance;
            var sortButtonManager = UISortButtonsManager.Instance;
            var sortController = SortController.Instance;
            var contextMenu = UIContextMenu.Instance;

            Debug.Log($"AudioFile.db located here {ConnectionString}");
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
