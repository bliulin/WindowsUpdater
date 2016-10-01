using System;

namespace Cegeka.Updater.Logic
{
    public interface IUpdateController
    {
        event EventHandler<EventArgs> Inactivated;
        void BeginUpdate();
        void Stop();
    }
}