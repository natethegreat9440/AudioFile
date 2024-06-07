using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GUIController : MonoBehaviour
{
    #region Variables
    public MediaPlayerManager mediaPlayerManager;
    public TMP_Text songNameText;
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
        nextButton.onClick.AddListener(PlayNextSong);
        previousButton.onClick.AddListener(PlayPreviousSong);

        // Subscribe to the OnTrackChanged event
        mediaPlayerManager.OnTrackChanged += UpdateSongName;

        // Update the song name initially
        UpdateSongName(mediaPlayerManager.audioSource.clip != null ? mediaPlayerManager.mediaLibrary.songs[mediaPlayerManager.currentSongIndex].name : "No Song");
    }

    private void OnDestroy()
    {
        // Unsubscribe from the OnTrackChanged event to prevent memory leaks
        mediaPlayerManager.OnTrackChanged -= UpdateSongName;
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
            mediaPlayerManager.Play();

            //Update UI to show Play Button
        }
        isPlaying = !isPlaying;
    }

    public void PlayNextSong()
    {
        mediaPlayerManager.NextSong();
    }

    public void PlayPreviousSong()
    {
        mediaPlayerManager.PreviousSong();
    }

    private void UpdateSongName(string trackName)
    {
        songNameText.text = trackName;
    }
}
