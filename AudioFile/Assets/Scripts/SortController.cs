﻿using System.Collections;
using System.Collections.Generic;
using AudioFile;
using System.Windows.Forms;
using AudioFile.Model;
using AudioFile.View;
using AudioFile.ObserverManager;
using System;
using UnityEngine;


namespace AudioFile.Controller
{
    public class SortController : MonoBehaviour, IController
    {
        // Lazy<T> ensures that the instance is created in a thread-safe manner
        private static readonly Lazy<SortController> _instance = new Lazy<SortController>(CreateSingleton);

        // Private constructor to prevent instantiation
        private SortController() { } //Unity objects should not use constructors with parameters as Unity uses its own lifecycle methods to manage these objects

        public static SortController Instance => _instance.Value;

        private static SortController CreateSingleton()
        {
            // Create a new GameObject to hold the singleton instance if it doesn't already exist
            GameObject singletonObject = new GameObject(typeof(SortController).Name);

            // Add the PlayBackController component to the GameObject
            return singletonObject.AddComponent<SortController>();
        }

        //TODO: Add reference variables for the Sorted State and Sorted Property??
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void HandleRequest(object request, bool isUndo)
        {
            //Add methods to log these commands with the UndoController
            SortCommand command = request as SortCommand;

            if (isUndo == false)
            {
                Action action = command.State switch
                {
                    SortButtonState.Forward => () => 
                    {
                        SortForward(command.CollectionToSort, command.SortBy);
                    },
                    SortButtonState.Reverse => () => 
                    {
                        SortReverse(command.CollectionToSort, command.SortBy);
                    },
                    SortButtonState.Default => () =>
                    {
                        RestoreDefaultOrder(command.CollectionToSort);
                    },
                    //Add more switch arms here as needed
                    _ => () => Debug.LogWarning($"Unhandled command: {request}")
                };

                action();
            }
            //Undo handling
            else
            {
                Action action = command.State switch
                {
                    SortButtonState.Forward => () =>
                    {
                        RestoreDefaultOrder(command.CollectionToSort);
                    },
                    SortButtonState.Reverse => () =>
                    {
                        SortForward(command.CollectionToSort, command.SortBy);
                    },
                    SortButtonState.Default => () =>
                    {
                        SortReverse(command.CollectionToSort, command.SortBy);
                    },
                    //Add more switch arms here as needed
                    _ => () => Debug.LogWarning($"Unhandled undo command: {request}")
                };

                action();
            }
        }

        private void SortForward(string collectionToSort, string sortProperty)
        {
            throw new NotImplementedException();
        }
        private void SortReverse(string collectionToSort, string sortProperty)
        {
            throw new NotImplementedException();
        }
        private void RestoreDefaultOrder(string collectionToSort)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}