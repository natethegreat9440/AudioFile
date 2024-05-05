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

        // Update the song name initially
        UpdateSongName();
    }

    private void Update()
    {
        // Continuously update the song name in case it changes
        if (mediaPlayerManager.audioSource.clip != null)
        {
            songNameText.text = mediaPlayerManager.audioSource.clip.name;
        }
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
        UpdateSongName();
    }

    public void PlayPreviousSong()
    {
        mediaPlayerManager.PreviousSong();
        UpdateSongName();
    }

    private void UpdateSongName()
    {
        if (mediaPlayerManager.audioSource.clip != null)
        {
            songNameText.text = mediaPlayerManager.audioSource.clip.name;
        }
    }
}
