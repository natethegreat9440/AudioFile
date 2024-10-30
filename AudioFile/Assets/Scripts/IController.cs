using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioFile.Controller
{
    public interface IController
    {
        void Initialize();

        void HandleRequest(object request, bool isUndo);

        void Dispose();

        //TODO: move Exit Program into a an Exit Controller class
        //void ExitProgram();

    }
}
