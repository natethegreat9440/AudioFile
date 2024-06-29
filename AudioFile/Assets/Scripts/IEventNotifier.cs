using System;

public interface IEventNotifier
{
    event EventHandler TrackChanged;
    event EventHandler PlayStateChanged;
    event EventHandler WriteStateChanged;
    event EventHandler ImportStageChanged;
    event EventHandler ExportStageChanged;
}
