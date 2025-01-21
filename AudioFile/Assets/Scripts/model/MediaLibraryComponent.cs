using UnityEngine;
using UnityEngine.Playables;
using System;

namespace AudioFile.Model
{
    /// <summary>
    /// Abstract base class for all Media Library Components, which are nodes in the Media Library tree. 
    /// <remarks>
    /// Likely to be deprecated class later as SQLite database will be the model for all Tracks and likely Visualizers. Once part of a Composite design pattern implementation. 
    /// TrackLibrary and VisualizerLibrary (tentative) were intended to be the composite nodes. Track and Visualizer are leaf nodes and Track still currently implements this interface. 
    /// May refactor as an simpler interface for media items.
    /// Members: _name, , Play(), Pause(), Stop(), GetDuration(), SetTime(), Skip(). Has default ToString() override implementations. A
    /// AddComponent(), RemoveComponent(), GetChild() methods removed as moving away from a composite design pattern. NextItem(), 
    /// PreviousItem(), AddItem(), RemoveItem(), RemoveItemAtIndex() are subject for removal.
    /// Implements GetHandle() from IPlayable (this may be subject for removable if I don't end up utilizing 
    /// Unity's PlayableGraph and PlayableHandle objects - Unity's AudioSource class seems to be giving me everything I need for Track objects thus far.) 
    /// Inherits MonoBehaviour, but no implementation overrides at this abstract level, however there are many implementations at concrete levels.
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IPlayable"/>
    /// </summary>
    public abstract class MediaLibraryComponent : MonoBehaviour, IPlayable
    {
        protected PlayableGraph _playableGraph;
        protected PlayableHandle _playableHandle;
        protected string _name; 
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
        #endregion
        #region Playback methods for tracks/visualizers
        public virtual void Play(string trackDisplayID)
        {
            throw new NotImplementedException();
        }
        public virtual void Pause(string trackDisplayID)
        {
            throw new NotImplementedException();
        }
        public virtual void Stop(string trackDisplayID)
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
        public virtual void AddItem(MediaLibraryComponent mediaLibraryComponent, bool isItemNew)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveItem(string trackID)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveItemAtIndex(int providedIndex)
        { 
            throw new NotImplementedException();
        }
        #endregion
    }
}
