using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using AudioFile.Model;
using AudioFile.ObserverManager;
using System.Collections;
using System.ComponentModel;
using AudioFile.Controller;
using TMPro;


namespace AudioFile.View
{
    public class UIPlaybackButtonsManager : MonoBehaviour, IAudioFileObserver
    {
        int trackDisplayIndex;
        void Start()
        {
            AudioFile.ObserverManager.ObserverManager.Instance.RegisterObserver("OnTrackSelected", this);

            Canvas canvas = GameObject.Find("GUI_Canvas").GetComponent<Canvas>();

            Button prevButton = canvas.transform.Find("Playback_Controls/Previous_Button").GetComponent<Button>();
            Button playButton = canvas.transform.Find("Playback_Controls/Play_Pause_Button").GetComponent<Button>();
            Button nextButton = canvas.transform.Find("Playback_Controls/Next_Button").GetComponent<Button>();
            Button stopButton = canvas.transform.Find("Playback_Controls/Stop_Button").GetComponent<Button>();

            prevButton.onClick.AddListener(() => AudioFile.Controller.PlaybackController.Instance.HandleRequest(new PreviousItemCommand()));
            nextButton.onClick.AddListener(() => AudioFile.Controller.PlaybackController.Instance.HandleRequest(new NextItemCommand()));

            stopButton.onClick.AddListener(() =>
            {
                if (PlaybackController.Instance.CurrentTrack != null)
                {
                    AudioFile.Controller.PlaybackController.Instance.HandleRequest(new StopCommand(trackDisplayIndex));
                }
            });

            // Assuming playbackController is a reference to your PlaybackController instance
            if (playButton != null)
            {
                playButton.onClick.AddListener(() =>
                {
                    if (PlaybackController.Instance.CurrentTrack == null)
                    {
                        // Play the selected track
                        PlaybackController.Instance.HandleRequest(new PlayCommand(trackDisplayIndex));
                    }
                    else if (PlaybackController.Instance.CurrentTrack != null && PlaybackController.Instance.GetCurrentTrackIndex() == trackDisplayIndex && PlaybackController.Instance.CurrentTrack.IsPlaying)
                    {
                        // Pause the current track
                        PlaybackController.Instance.HandleRequest(new PauseCommand(trackDisplayIndex));
                    }
                    else
                    {
                        // Play the selected track
                        PlaybackController.Instance.HandleRequest(new PlayCommand(trackDisplayIndex));
                    }
                });
            }

        }
        public void AudioFileUpdate(string observationType, object data)
        {
            if (observationType == "OnTrackSelected")
            {
                trackDisplayIndex = (int)data;
            }
        }
    }
}
