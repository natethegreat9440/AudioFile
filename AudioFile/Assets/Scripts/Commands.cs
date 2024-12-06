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
        public string TrackDisplayID { get; }


        public PlayCommand(string trackDisplayID)
        {
            TrackDisplayID = trackDisplayID;
        }

        public void Execute()
        {
            Debug.Log("New Track Command executed");
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }

        public void Undo()
        {
            Debug.Log("New Track Command undone");
            IsUndo = true;
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }
    }

    public class PauseCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        public string TrackDisplayID { get; }


        public PauseCommand(string trackDisplayID)
        {
            TrackDisplayID = trackDisplayID;
        }

        public void Execute()
        {
            Debug.Log("New Pause Command executed");
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }

        public void Undo()
        {
            Debug.Log("New Pause Command undone");
            IsUndo = true;
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }
    }

    public class SeekCommand : ICommand
    {
        public bool IsUndo { get; set; } = false; //Seek Command can only be undone if it is at the top of the Command Stack. Undo only exists to in case a track was accidentally seeked by user error

        public float PreviousTime { get; set; }

        public float NewTime { get; set; }
        public SeekCommand(float previousTime, float newTime)
        {
            PreviousTime = previousTime;
            NewTime = newTime;
        }
        public void Execute()
        {
            Debug.Log("Seek Command executed");
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }
        public void Undo()
        {
            Debug.Log("Seek Command undone");
            IsUndo = true;
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }
    }

    public class StopCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        public string TrackDisplayID { get; }

        public StopCommand(string trackDisplayID)
        {
            TrackDisplayID = trackDisplayID;
        }

        public void Execute()
        {
            Debug.Log("New Stop Command executed");
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }

        public void Undo()
        {
            Debug.Log("New Stop Command undone");
            IsUndo = true;
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }
    }

    public class NextItemCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        public void Execute()
        {
            Debug.Log("New Next Item Command executed");
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }

        public void Undo()
        {
            Debug.Log("New Next Item Command undone");
            IsUndo = true;
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }
    }

    public class PreviousItemCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        public void Execute()
        {
            Debug.Log("New Previous Item Command executed");
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }

        public void Undo()
        {
            Debug.Log("New Previous Item Command undone");
            IsUndo = true;
            PlaybackController.Instance.HandleRequest(this, IsUndo);
        }
    }
    public class AddTrackCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        public List<Track> Tracks { get; set; } = new List<Track>();

        public void Execute()
        {
            Debug.Log("New Track Command executed");
            TrackLibraryController.Instance.HandleRequest(this, IsUndo);
        }

        public void Undo()
        {
            Debug.Log("New Track Command undone");
            IsUndo = true;
            TrackLibraryController.Instance.HandleRequest(this, IsUndo);
        }
    }

    public class RemoveTrackCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        //public Track Track { get; }

        //public int Index { get; }
        public List<string> TrackDisplayIDs { get; }

        public List<string> Paths { get; set; } = new List<string>();

        public List<Dictionary<string, string>> TrackProperties { get; set; } = new List<Dictionary<string, string>>(); //Has ability to hold Track Properties for multiple tracks

        public RemoveTrackCommand(List<string> trackDisplayIDs)
        {
            TrackDisplayIDs = trackDisplayIDs;
        }
        public void Execute()
        {
            Debug.Log("Remove Track Command executed");
            TrackLibraryController.Instance.HandleRequest(this, IsUndo);
        }

        public void Undo()
        {
            Debug.Log("Remove Track Command undone");
            IsUndo = true;
            TrackLibraryController.Instance.HandleRequest(this, IsUndo);
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

    public class AddToPlaylistCommand : ICommand
    {
        public bool IsUndo { get; set; } = false;

        //Will need to have a reference to the playlist once I have that class figured out
        //public Playlist Playlist { get; }

        public List<string> TrackDisplayIDs { get; set; } //Track reference used for execute and undo action

        public AddToPlaylistCommand(List<string> trackDisplayIDs)//,Playlist playlist)
        {
            //Playlist = playlist;
            TrackDisplayIDs = trackDisplayIDs;
        }

        public void Execute()
        {
            //controller.HandleRequest(this, IsUndo);
            //Debug.Log("New Add To Playlist Command executed");
            throw new NotImplementedException();
        }

        public void Undo()
        {
            throw new NotImplementedException();
            //controller.HandleRequest(this, IsUndo = true);
            //Debug.Log("New Playlist Command undone");
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
