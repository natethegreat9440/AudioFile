using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
//using UnityEngine.UIElements; Having this line in causes ambiguous references for Buttons etc. This library is for newer buttons whereas UnityEngine.UI has all of the legacy UI elements

namespace AudioFile
{
    public interface ICommand
    {
        void Execute();
    }

    public abstract class MenuComponent
    {
        protected bool _enabled = true;
        public virtual void Add(MenuComponent menuComponent)
        {
            throw new NotImplementedException();
        }

        public virtual void Remove(MenuComponent menuComponent)
        {
            throw new NotImplementedException();
        }

        public virtual MenuComponent GetChild(int i)
        {
            throw new NotImplementedException();
        }

        public virtual string GetName()
        {
            throw new NotImplementedException();
        }

        public virtual string GetDescription()
        {
            throw new NotImplementedException();
        }

        protected bool IsEnabled()
        {
            if (_enabled == true) return true;
            else return false;
        }

        protected void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        public virtual void Display()
        {
            throw new NotImplementedException();
        }

        public virtual void Hide()
        {
            throw new NotImplementedException();
        }

        public virtual void ExecuteAction()
        {
            throw new NotImplementedException();
        }

        public virtual void MenuItem_Click()
        {
            throw new NotImplementedException();
        }

        /*public virtual void Menu_MouseEnter()
        {
            throw new UnsupportedOperationException();
        }*/
    }

    public class MenuItem : MenuComponent
    {
        string _name;
        string _description;
        Button _button;
        ICommand _command;

        public MenuItem(string name, string description, Button button, ICommand command)
        {
            _name = name;
            _description = description;
            _button = button;
            _button.GetComponentInChildren<Text>().text = name;
            _button.onClick.AddListener(MenuItem_Click); // Wire up the event handler
            _command = command;
        }

        public override string ToString()
        {
            return $"{_name} - {_description}";
        }

        public override string GetName()
        {
            return _name;
        }

        public override string GetDescription()
        {
            return _description;
        }

        public override void Display()
        {
            if (_enabled == true)
            {
                _button.gameObject.SetActive(true); //set button when Display is called
            }
            else
            {
                _button.enabled = false;
            }
        }

        public override void Hide()
        {
            _button.gameObject.SetActive(false); //set button when Hide is called
        }

        public override void ExecuteAction()
        {
            if (_enabled && _command != null)
            {
                _command.Execute();
            }
        }

        public override void MenuItem_Click()
        {
            this.ExecuteAction();
        }

    }

    public class Menu : MenuComponent, IPointerEnterHandler, IPointerExitHandler
    {
        List<MenuComponent> _menuComponents = new List<MenuComponent>();
        string _name;
        string _description;
        Text _text;

        public Menu(string name, string description, Text text)
        {
            _name = name;
            _description = description;
            _text = text;
            _text.text = name;

            // Enable Raycast Target for hover detection
            _text.raycastTarget = true;

            // Attach the event handling to the Text GameObject
            if (_text.gameObject.GetComponent<EventTrigger>() == null)
            {
                _text.gameObject.AddComponent<EventTrigger>();
            }
        }


        public override void Add(MenuComponent menuComponent) => _menuComponents.Add(menuComponent);
        public override void Remove(MenuComponent menuComponent) => _menuComponents.Remove(menuComponent);
        public override MenuComponent GetChild(int i) => _menuComponents[i];

        public override string GetName() => _name;
        public override string GetDescription() => _description;


        public override void Display()
        {
            if (_enabled == true)
            {
                _text.gameObject.SetActive(true); //set button when Display is called
                foreach (var component in _menuComponents)
                {
                    component.Display();
                }
            }
            else
            {
                _text.enabled = false;
            }
        }

        public override void Hide() // Hide method to undisplay the menu components (called on OnPointerExit)
        {
            foreach (var component in _menuComponents)
            {
                component.Hide(); // Assuming child components have a Hide() method
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            this.Display();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            this.Hide();
        }
    }





    /*public class GUIController : MonoBehaviour
    {
        #region Variables
        public MediaPlayerManager mediaPlayerManager;
        public TMP_Text trackNameText;
        public Button playButton;
        public Button nextButton;
        public Button previousButton;

        private bool isPlaying = true;
        #endregion

        #region UnityEngine
        private void Start()
        {
            // Set up button listeners
            playButton.onClick.AddListener(TogglePlay);
            nextButton.onClick.AddListener(PlayNextTrack);
            previousButton.onClick.AddListener(PlayPreviousTrack);

            // Subscribe to the OnTrackChanged event
            mediaPlayerManager.OnTrackChanged += UpdateTrackName;

            // Update the track name initially. The ? is used to check if the condition (part before ?) is true. If true it returns the part before the ":" sign and if false it does what is after
            UpdateTrackName(mediaPlayerManager.audioSource.clip != null ? mediaPlayerManager.mediaLibrary.tracks[mediaPlayerManager.currentTrackIndex].Name : "No Track");
        }

        private void Update()
        {
            // Continuously update the song name in case it changes
            if (mediaPlayerManager.audioSource.clip != null)
            {
                trackNameText.text = mediaPlayerManager.audioSource.clip.name;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from the OnTrackChanged event to prevent memory leaks
            mediaPlayerManager.OnTrackChanged -= UpdateTrackName;
        }
        #endregion

        public void TogglePlay()
        {
            if (isPlaying)
            {
                mediaPlayerManager.Pause();

                //Update UI to show Pause Button
            }
            else
            {
                mediaPlayerManager.PlayCurrentTrack();

                //Update UI to show Play Button
            }
            isPlaying = !isPlaying;
        }

        public void PlayNextTrack()
        {
            mediaPlayerManager.NextTrack();
            //UpdateTrackName();
        }

        public void PlayPreviousTrack()
        {
            mediaPlayerManager.PreviousTrack();
            //UpdateTrackName();
        }

        //TODO: Update method so it displays track Title (from MP3 metadata) as opposed to track/clip name which is essentially the file name. Display file name if no Track metadata field is found. See ChatGPT convo
        private void UpdateTrackName(string trackName)
        {
            if (mediaPlayerManager.audioSource.clip != null)
            {
                trackNameText.text = trackName;
                //trackNameText.text = mediaPlayerManager.audioSource.clip.name;
                //trackNameText.text = trackName;
            }
        }
    }*/
}