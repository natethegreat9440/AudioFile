using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Mono.Data.Sqlite;


namespace AudioFile.Controller
{
    /// <summary>
    /// Generic singleton controller for controlling the exit of the application when Exit command is executed
    /// <remarks>
    /// Members: ExitAudioFile() with options for specific Unity Editor and standalone program logic. DropTracksTable() for testing purposes commented out - useful for easily resetting the Tracks table to empty.
    /// Implements HandleRequest() from IController. Initialize() and Dispose() are not implemented from MonoBehaviour.
    /// Interfaces can't inherit from MonoBehaviour. Usually requests are Command objects, but don't have to be.
    /// Bool isUndo specifies if the Command passed should execute it's Undo() operation if true
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IController"/>
    /// </summary>

    public class ExitProgramController : MonoBehaviour, IController
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<ExitProgramController> _instance = new Lazy<ExitProgramController>(CreateSingleton);

        // Private constructor to prevent instantiation
        private ExitProgramController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static ExitProgramController Instance => _instance.Value;

        private static ExitProgramController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(ExitProgramController).Name);

            // Add the PlayBackController component to the GameObject
            return singletonObject.AddComponent<ExitProgramController>();
        }

        public string ConnectionString => SetupController.Instance.ConnectionString;

        public void HandleRequest(object request, bool isUndo)
        {
            string command = request.GetType().Name;

            Action action = (command, isUndo) switch
            {
                ("ExitProgramCommand", false) => () =>
                {
                    Debug.Log("Exit Program Command handled");
                    ExitAudioFile();
                }
                ,
                //Add more switch arms here as needed
                _ => () => Debug.LogWarning($"Unhandled command: {command} at {this}")
            };

            action();
        }

        void ExitAudioFile()
        {
            #if UNITY_EDITOR //This if statement only occurs while using the Unity Editor to simulate closing the application. 
            //Application.Quit() does not work in the editor, but will work in a standalone build. Hence, the special usage of # syntax
                    // Stop play mode in the editor
                    Debug.Log("Exiting UNITY_EDITOR.");
                    //DropTracksTable(); //Use for testing - comment out otherwise
                    UnityEditor.EditorApplication.isPlaying = false;

            #else
                    // Quit the application
                    Application.Quit();
            #endif
        }

        //private void DropTracksTable() //Test method
        //{
        //    using (var connection = new SqliteConnection(ConnectionString))
        //    {
        //        connection.Open();
        //        string dropTableQuery = "DROP TABLE IF EXISTS Tracks";
        //        using (SqliteCommand command = new SqliteCommand(dropTableQuery, connection))
        //        {
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //    Debug.Log("Tracks table dropped.");
        //}
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
