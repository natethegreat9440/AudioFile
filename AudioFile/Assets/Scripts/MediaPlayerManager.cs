using UnityEngine;

public enum PlayMode { Consecutive, RecommendedRandom, TrueRandom }

public class MediaPlayerManager : MonoBehaviour
{
    #region Variables
    public AudioSource audioSource; // Unity's AudioSource component for playing music
    public MediaLibrary mediaLibrary;
    private int currentSongIndex = 0;

    // Events to communicate with other components
    public delegate void TrackChangeHandler(string trackName);
    public event TrackChangeHandler OnTrackChanged;

    public delegate void PlayStateChangeHandler(bool isPlaying);
    public event PlayStateChangeHandler OnPlayStateChanged;

    private PlayMode currentPlayMode = PlayMode.Consecutive;
    #endregion

    #region UnityEngine
    private void Start()
    {
        if (audioSource == null || mediaLibrary == null || mediaLibrary.songs.Length == 0)
        {
            Debug.LogError("MediaPlayerManager setup error: Ensure AudioSource and MediaLibrary are set and MediaLibrary is not empty.");
            this.enabled = false; // Disable the script if setup is incomplete
        }
        else
        {
            PlayCurrentSong();
        }
    }
    #endregion

    #region TestingMethods
    public void PlayCurrentSong()
    {
        if (mediaLibrary.songs.Length > 0 && currentSongIndex < mediaLibrary.songs.Length)
        {
            audioSource.clip = mediaLibrary.songs[currentSongIndex].clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("No songs available or index out of range.");
        }
    }
    public void NextSong()
    {
        if (currentSongIndex < mediaLibrary.songs.Length - 1)
        {
            currentSongIndex++;
            PlayCurrentSong();
        }
        else
        {
            Debug.Log("Reached the end of the playlist.");
        }
    }
    #endregion
    public void PreviousSong()
    {
        if (currentSongIndex > 0)
        {
            currentSongIndex--;
            PlayCurrentSong();
        }
        else
        {
            Debug.Log("Already at the beginning of the playlist.");
        }
    }

    public void Play()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log("Audio was Played");

            OnPlayStateChanged?.Invoke(true);
            //// Check if the OnPlayStateChanged event has any subscribers
            //if (OnPlayStateChanged != null)
            //{
            //    // If there are subscribers, invoke the event and pass 'true' to indicate the player is now playing
            //    OnPlayStateChanged.Invoke(true);
            //}
        }
    }

    public void Pause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("Audio was Paused");

            OnPlayStateChanged?.Invoke(false);
        }
    }

    public void Skip(bool forward)
    {
        // This is a placeholder for how you might handle track skipping
        if (forward)
        {
            // Move to the next track
            Debug.Log("Skipping to next track");
        }
        else
        {
            // Move to the previous track
            Debug.Log("Skipping to previous track");
        }

        // Trigger the track change event
        OnTrackChanged?.Invoke(audioSource.clip.name);
        Play(); // Play the new track
    }

    public void SetPlayMode(PlayMode mode)
    {
        currentPlayMode = mode;
        Debug.Log($"Play mode changed to: {mode}");
    }
}
