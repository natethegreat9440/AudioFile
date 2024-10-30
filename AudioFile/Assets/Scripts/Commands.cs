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

    public class AddTrackCommand : ICommand
    {
        private TrackLibraryController controller;

        bool isUndo = false;
        
        public AddTrackCommand(TrackLibraryController controller)
        {
            this.controller = controller;
        }
        public void Execute()
        {
            controller.HandleRequest(this, isUndo);
            Debug.Log("New Track Command executed");
        }

        public void Undo()
        {
            controller.HandleRequest(this, isUndo=true);
            Debug.Log("New Track Command undone");
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
