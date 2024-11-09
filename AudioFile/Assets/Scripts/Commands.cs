using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEditor;
using AudioFile.Controller;
using AudioFile.Model;
using System;
using UnityEngine;

namespace AudioFile.View
{
    /// <summary>
    /// This file contains all the Commands used in AudioFile
    /// <remarks>
    /// All the commands are ICommand objects used to execute and undo user specified actions. Each command has an Execute() and Undo() method.
    /// Undo() is always designed to do the opposite action of Execute() for a given command and thus certain commands may need to have
    /// keep track of certain Properties that may be necessary for undoing the command later as needed. Controllers however are what implement the actual 
    /// logic for executing and undoing commands based on the type of command passed and whether the command should be executed or undone (i.e., IsUndo gets set to true by the CommandStackController).
    /// Not every command has an undo action.
    /// </remarks>
    /// <see cref="CommandStackController"/>
    /// </summary>

    //TODO:Implement undo and redo commands (redo might be handled by a command controller keeping a fixed stack of commands)
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class PlayCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;
        public int Index { get; }

        public PlayCommand(int index)
        {
            Index = index;
        }

        public void Execute()
        {
            PlaybackController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Track Command executed");
        }

        public void Undo()
        {
            IsUndo = true;
            PlaybackController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Track Command undone");
        }
    }

    public class PauseCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;
        public int Index { get; }

        public PauseCommand(int index)
        {
            Index = index;
        }

        public void Execute()
        {
            PlaybackController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Pause Command executed");
        }

        public void Undo()
        {
            IsUndo = true;
            PlaybackController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Pause Command undone");
        }
    }

    public class StopCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        public int Index { get; }

        public StopCommand(int index)
        {
            Index = index;
        }

        public void Execute()
        {
            PlaybackController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Stop Command executed");
        }

        public void Undo()
        {
            IsUndo = true;
            PlaybackController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Stop Command undone");
        }
    }

    public class NextItemCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        public void Execute()
        {
            PlaybackController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Next Item Command executed");
        }

        public void Undo()
        {
            IsUndo = true;
            PlaybackController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Next Item Command undone");
        }
    }

    public class PreviousItemCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        public void Execute()
        {
            PlaybackController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Previous Item Command executed");
        }

        public void Undo()
        {
            IsUndo = true;
            PlaybackController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Previous Item Command undone");
        }
    }
    public class AddTrackCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        public Track Track { get; set; }

        public void Execute()
        {
            TrackLibraryController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Track Command executed");
        }

        public void Undo()
        {
            IsUndo = true;
            TrackLibraryController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("New Track Command undone");
        }
    }

    public class RemoveTrackCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        //public Track Track { get; }

        public int Index { get; }

        public string Path { get; set; }

        public RemoveTrackCommand(int index)
        {
            Index = index;
        }
        public void Execute()
        {
            TrackLibraryController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("Remove Track Command executed");
        }

        public void Undo()
        {
            IsUndo = true;
            TrackLibraryController.Instance.HandleRequest(this, IsUndo);
            Debug.Log("Remove Track Command undone");
        }
    }

    public class AddPlaylistCommand : ICommand
    {
        /* TODO: Implement this later once Controller is implemented
        private IController controller;
        public AddPlaylistCommand(IController controller)
        {
            this.controller = controller;
        }
        */
        public void Execute()
        {
            //controller.AddEmptyPlaylist();
            Debug.Log("New Playlist Command executed");
            throw new NotImplementedException();
        }

        public void Undo()
        {
            throw new NotImplementedException();
            //controller.HandleRequest(this, IsUndo = true);
            //Debug.Log("New Playlist Command undone");
        }
    }

    public class AddPlaylistFolderCommand : ICommand
    {
        /* TODO: Implement this later once Controller is implemented
        private IController controller;
        public AddPlaylistFolderCommand(IController controller)
        {
            this.controller = controller;
        }
        */
        public void Execute()
        {
            //controller.AddEmptyPlaylistFolder();
            Debug.Log("New Playlist Folder Command executed");
            throw new NotImplementedException();
        }

        public void Undo()
        {
            throw new NotImplementedException();
            //controller.HandleRequest(this, IsUndo = true);
            //Debug.Log("New Playlist Folder Command undone");
        }
    }


    public class ExitProgramCommand : ICommand
    {
        /* TODO: Implement this later once Controller is implemented
        private IController controller;
        public ExitProgramCommand(IController controller)
        {
            this.controller = controller;
        }
        */
        public void Execute()
        {
            //controller.ExitProgram();
            Debug.Log("Exit Program Command executed");
            throw new NotImplementedException();
        }

        public void Undo()
        {
            throw new NotImplementedException();
            //this command will have no undo action, perhaps does't need to be setup as a command
        }
    }
}
