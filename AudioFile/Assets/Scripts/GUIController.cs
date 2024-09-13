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

        // Update the song name initially. The ? is used to check if the condition (part before ?) is true. If true it returns the part before the ":" sign and if false it does what is after
        UpdateSongName(mediaPlayerManager.audioSource.clip != null ? mediaPlayerManager.mediaLibrary.songs[mediaPlayerManager.currentSongIndex].name : "No Song");
    }

    private void Update()
    {
        // Continuously update the song name in case it changes
        if (mediaPlayerManager.audioSource.clip != null)
        {
            songNameText.text = mediaPlayerManager.audioSource.clip.name;
        }
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
            mediaPlayerManager.PlayCurrentSong();

            //Update UI to show Play Button
        }
        isPlaying = !isPlaying;
    }

    public void PlayNextSong()
    {
        mediaPlayerManager.NextSong();
        //UpdateSongName();
    }

    public void PlayPreviousSong()
    {
        mediaPlayerManager.PreviousSong();
        //UpdateSongName();
    }

    //TODO: Update method so it displays song Title (from MP3 metadata) as opposed to track/clip name which is essentially the file name. Display file name if no Track metadata field is found. See ChatGPT convo
    private void UpdateSongName(string trackName)
    {
        if (mediaPlayerManager.audioSource.clip != null)
        {
            songNameText.text = trackName;
            //songNameText.text = mediaPlayerManager.audioSource.clip.name;
            //songNameText.text = trackName;
        }
    }
}
