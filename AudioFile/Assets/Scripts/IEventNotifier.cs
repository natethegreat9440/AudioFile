using System;
using System.Collections;
using System.Collections.Generic;

public interface IEventNotifier
{
    event EventHandler TrackChanged;
    event EventHandler PlayStateChanged;
    event EventHandler WriteStateChanged;
    event EventHandler ImportStageChanged;
    event EventHandler ExportStageChanged;
}
