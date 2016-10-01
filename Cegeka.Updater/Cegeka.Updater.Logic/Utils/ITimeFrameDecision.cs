using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cegeka.Updater.Logic.Configuration.Model;

namespace Cegeka.Updater.Logic.Utils
{
    public interface ITimeFrameDecision
    {
        bool IsWithinTimeFrame(TimeFrame timeFrame);
    }
}
