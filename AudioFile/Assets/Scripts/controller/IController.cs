using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioFile.Controller
{
    /// <summary>
    /// Generic interface for controllers in AudioFile
    /// <remarks>
    /// Members: HandleUserRequest(). Initialize() and Dispose() are intended to be used as MonoBehaviour methods.
    /// Interfaces can't inherit from MonoBehaviour. Usually requests are Command objects, but don't have to be.
    /// Bool isUndo specifies if the Command passed should execute it's Undo() operation if true
    /// </remarks>
    /// <see cref="MonoBehaviour"/>
    /// <seealso cref="IAudioFileObserver"/>
    /// </summary>

    public interface IController
    {
        void Initialize();

        void HandleUserRequest(object request, bool isUndo);

        void Dispose();

        //TODO: move Exit Program into a an Exit Controller class

    }
}
