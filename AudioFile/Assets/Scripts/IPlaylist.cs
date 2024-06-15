//Create an interface called IPlaylist that extends Ilibrary. It should have methods for AddPlaylistTitle(), AddPlaylistDescription(), GetPlaylistTitle(), GetPlaylistDescription() and other Playlist specific methods.

public interface IPlaylist : ILibrary
{
    void AddPlaylistTitle(string title);
    void AddPlaylistDescription(string description);
    string GetPlaylistTitle();
    string GetPlaylistDescription();
    float GetPlaylistAverageBPM();
    void SortByBPMPattern(BPMSortPattern sortPattern);

}