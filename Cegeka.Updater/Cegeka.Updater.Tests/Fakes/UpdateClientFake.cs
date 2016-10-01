using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cegeka.Updater.Logic.Installation;
using WUApiLib;

namespace Cegeka.Updater.Tests.Fakes
{
  class UpdateClientFake : IUpdateClient
  {
    public WUApiLib.IUpdateCollection GetAvailableUpdates()
    {
      System.Threading.Thread.Sleep(100);
      return new UpdateCollectionClass();
    }

    public Result InstallUpdates(WUApiLib.IUpdateCollection updates)
    {
      throw new NotImplementedException();
    }
  }
}
