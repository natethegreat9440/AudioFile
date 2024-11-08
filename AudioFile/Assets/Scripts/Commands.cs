using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using AudioFile.Controller;
using AudioFile.Model;
using UnityEditor;


namespace AudioFile.View
{
    //TODO:Implement undo and redo commands (redo might be handled by a command controller keeping a fixed stack of commands)
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class PlayCommand : ICommand
    {
        bool isUndo = false;
        public int Index { get; }

        public PlayCommand(int index) 
        {
            Index = index;
        }

        public void Execute()
        {
            PlaybackController.Instance.HandleRequest(this, isUndo);
            Debug.Log("New Track Command executed");
        }

        public void Undo()
        {
            PlaybackController.Instance.HandleRequest(this, isUndo = true);
            Debug.Log("New Track Command undone");
        }
    }

    public class PauseCommand : ICommand
    {
        bool isUndo = false;
        public int Index { get; }

        public PauseCommand(int index)
        {
            Index = index;
        }

        public void Execute()
        {
            PlaybackController.Instance.HandleRequest(this, isUndo);
            Debug.Log("New Pause Command executed");
        }

        public void Undo()
        {
            PlaybackController.Instance.HandleRequest(this, isUndo = true);
            Debug.Log("New Pause Command undone");
        }
    }

    public class StopCommand : ICommand
    {
        bool isUndo = false;

        public int Index { get; }

        public StopCommand(int index)
        {
            Index = index;
        }

        public void Execute()
        {
            PlaybackController.Instance.HandleRequest(this, isUndo);
            Debug.Log("New Stop Command executed");
        }

        public void Undo()
        {
            PlaybackController.Instance.HandleRequest(this, isUndo = true);
            Debug.Log("New Stop Command undone");
        }
    }

    public class NextItemCommand : ICommand
    {
        bool isUndo = false;

        public void Execute()
        {
            PlaybackController.Instance.HandleRequest(this, isUndo);
            Debug.Log("New Next Item Command executed");
        }

        public void Undo()
        {
            PlaybackController.Instance.HandleRequest(this, isUndo = true);
            Debug.Log("New Next Item Command undone");
        }
    }

    public class PreviousItemCommand : ICommand
    {
        bool isUndo = false;

        public void Execute()
        {
            PlaybackController.Instance.HandleRequest(this, isUndo);
            Debug.Log("New Previous Item Command executed");
        }

        public void Undo()
        {
            PlaybackController.Instance.HandleRequest(this, isUndo = true);
            Debug.Log("New Previous Item Command undone");
        }
    }
    public class AddTrackCommand : ICommand
    {
        bool isUndo = false;

        public Track Track { get; set; }

        public void Execute()
        {
            TrackLibraryController.Instance.HandleRequest(this, isUndo);
            Debug.Log("New Track Command executed");
        }

        public void Undo()
        {
            TrackLibraryController.Instance.HandleRequest(this, isUndo=true);
            Debug.Log("New Track Command undone");
        }
    }

    public class RemoveTrackCommand : ICommand
    {
        bool isUndo = false;

        //public Track Track { get; }

        public int Index { get; }

        public string Path { get; set; }

        public RemoveTrackCommand(int index)
        {
            Index = index;
        }
        public void Execute()
        {
            TrackLibraryController.Instance.HandleRequest(this, isUndo);
            Debug.Log("Remove Track Command executed");
        }

        public void Undo()
        {
            TrackLibraryController.Instance.HandleRequest(this, isUndo = true);
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
            //controller.HandleRequest(this, isUndo = true);
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
            //controller.HandleRequest(this, isUndo = true);
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
