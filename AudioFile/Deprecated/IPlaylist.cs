﻿//Create an interface called IPlaylist that extends Ilibrary. It should have methods for AddPlaylistTitle(), AddPlaylistDescription(), GetPlaylistTitle(), GetPlaylistDescription() and other Playlist specific methods.

using System;
using System.Collections;
using System.Collections.Generic;

public interface IPlaylist<T> : ILibrary<T>, IPlayable<T>
{
    public enum BPMSortPattern
    {
        Ascending,
        Descending,
        Peak,
        Valley,
        // Add any other specific sorting patterns you need
    }
    void AddPlaylistTitle(string title);
    void AddPlaylistDescription(string description);
    string GetPlaylistTitle();
    string GetPlaylistDescription();
    float GetPlaylistAverageBPM();
    void SortByBPMPattern(BPMSortPattern sortPattern);

}