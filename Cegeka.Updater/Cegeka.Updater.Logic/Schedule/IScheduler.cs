using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cegeka.Updater.Logic.Schedule
{
    public interface IScheduler
    {
        ScheduleSettings Settings { get; set; }
        bool Execute(Func<bool> action);
    }
}
