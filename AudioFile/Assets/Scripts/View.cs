using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace AudioFile
{
    public interface ICommand
    {
        void Execute();
    }

    public abstract class MenuComponent
    {
        bool _enabled = true;
        public void Add(MenuComponent menuComponent)
        {
            throw new UnsupportedOperationException();
        }

        public void Remove(MenuComponent menuComponent)
        {
            throw new UnsupportedOperationException();
        }

        public MenuComponent GetChild(int i)
        {
            throw new UnsupportedOperationException();
        }

        public void GetName()
        {
            throw new UnsupportedOperationException();
        }

        public void GetDescription()
        {
            throw new UnsupportedOperationException();
        }

        bool IsEnabled()
        {
            if (_enabled == true) return true;
            else return false;
        }

        void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        public void Display()
        {
            throw new UnsupportedOperationException();
        }

        public void ExecuteAction()
        { 
            throw new UnsupportedOperationException();
        }

        public void MenuItem_Click(object sender, MouseEventArgs e)
        {
            throw new UnsupportedOperationException();
        }

        public void Menu_MouseEnter(object sender, EventArgs e)
        {
            throw new UnsupportedOperationException();
        }
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
            _button.Click += MenuItem_Click; // Wire up the event handler
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

        public void override Display()
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

        public void override ExecuteAction()
        {
            if (enabled && command != null)
            {
                command.Execute();
            }
        }

        public override void MenuItem_Click(object sender, MouseEventArgs e)
        {
            this.ExecuteAction();
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