﻿//Create an interface called IPlaylist that extends Ilibrary. It should have methods for AddPlaylistTitle(), AddPlaylistDescription(), GetPlaylistTitle(), GetPlaylistDescription() and other Playlist specific methods.

using System.Collections.Generic;

public interface IPlaylist<T> : IList<T>
{
    void AddPlaylistTitle(string title);
    void AddPlaylistDescription(string description);
    string GetPlaylistTitle();
    string GetPlaylistDescription();
    float GetPlaylistAverageBPM();
    void SortByBPMPattern(BPMSortPattern sortPattern);

}