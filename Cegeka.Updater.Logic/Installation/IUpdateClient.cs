using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WUApiLib;

namespace Cegeka.Updater.Logic.Installation
{
  public interface IUpdateClient
  {
    IUpdateCollection GetAvailableUpdates();

    Result InstallUpdates(IUpdateCollection updates);
  }
}
