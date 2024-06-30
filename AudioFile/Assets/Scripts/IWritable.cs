/* Create an IWritable interface to be implemented CDWriter and RecordWriter class. 
/ This interface will have a Play method that will be called by the MediaPlayerManager class. It will also have a Pause() method and a Stop() method
*/
using System;
using System.Collections;
using System.Collections.Generic;

public interface IWritable
{
        void InitializeWrite();
        void WriteData(byte[] data);
        void FinalizeWrite();
        float GetWriteProgress();
}