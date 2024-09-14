using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GUIController : MonoBehaviour
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
}
