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


namespace AudioFile.View
{
    //TODO:Implement undo and redo commands
    public interface ICommand
    {
        void Execute();
    }

    public class AddTrackCommand : ICommand
    {
        private IController controller;
        public AddTrackCommand(IController controller)
        {
            this.controller = controller;
        }
        
        public void Execute()
        {
            controller.LoadItem();
            Debug.Log("New Track Command executed");
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
    }
}
