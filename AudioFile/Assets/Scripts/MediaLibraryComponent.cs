﻿using AudioFile.View;
using OpenCover.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using AudioFile.ObserverManager;
using AudioFile.Controller;

namespace AudioFile.Model
{
    public abstract class MediaLibraryComponent : MonoBehaviour, IPlayable
    {
        protected PlayableGraph _playableGraph;
        protected PlayableHandle _playableHandle;
        protected string _name; //_name primarily to identify the TrackLibrary and other composites. _name for a Track is just "A track"
        //private IController controller;
        public MediaLibraryComponent(string name="A Media Library Component")
        {
            _name = name;
        }
        #region Structural/setup methods
        public PlayableHandle GetHandle()
        {
            return _playableHandle;
        }
        public override string ToString()
        {
            return $"{_name}";
        }

        public virtual void AddComponent(MediaLibraryComponent mediaLibraryComponent)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveComponent(MediaLibraryComponent mediaLibraryComponent)
        {
            throw new NotImplementedException();
        }

        public virtual MediaLibraryComponent GetChild(int i)
        {
            throw new NotImplementedException();
        }

        #endregion
        #region Playback methods for tracks/visualizers
        public virtual void Play(int index = 0)
        {
            throw new NotImplementedException();
        }
        public virtual void Pause(int index = 0)
        {
            throw new NotImplementedException();
        }
        public virtual void Stop(int index = 0)
        {
            throw new NotImplementedException();
        }
        public virtual float GetDuration()
        {
            throw new NotImplementedException();
        }

        public virtual void SetTime(float time)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region Playback methods for TrackLibrary
        public virtual void Skip(int index = 0)
        {
            throw new NotImplementedException();
        }

        public virtual void NextItem() //Used as either next song or next visualizer
        {
            throw new NotImplementedException();
        }

        public virtual void PreviousItem() //Used as either previous song or previous visualizer
        {
            throw new NotImplementedException();
        }
        #endregion
        #region Model control methods
        public virtual void LoadItem()
        {
            throw new NotImplementedException();
        }
        public virtual void AddItem(MediaLibraryComponent mediaLibraryComponent)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveItem(MediaLibraryComponent mediaLibraryComponent)
        {
            throw new NotImplementedException();
        }
        #endregion


    }
}