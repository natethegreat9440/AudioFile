using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AudioFile.Controller
{
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

        void ExitAudioFile()
        {
            #if UNITY_EDITOR //This if statement only occurs while using the Unity Editor to simulate closing the application. 
            //Application.Quit() does not work in the editor, but will work in a standalone build. Hence, the special usage of # syntax
                    // Stop play mode in the editor
                    UnityEditor.EditorApplication.isPlaying = false;
            #else
                    // Quit the application
                    Application.Quit();
            #endif
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void HandleRequest(object request, bool isUndo)
        {
            string command = request.GetType().Name;

            Action action = (command, isUndo) switch
            {
                ("ExitProgramCommand", false) => () =>
                {
                    //Debug.Log("Exit Program Command handled");
                    ExitAudioFile();
                },
                //Add more switch arms here as needed

                _ => () => Debug.LogWarning($"Unhandled command: {command} at {this}")
            };

            action();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
