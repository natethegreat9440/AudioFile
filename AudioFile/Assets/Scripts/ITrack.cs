
using System;
using System.Collections;
using System.Collections.Generic;

public interface ITrack<T> : IPlayable<T>
{
    void UpdateMetadata();
    void ClearMetadata();

}