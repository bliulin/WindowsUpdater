using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cegeka.Updater.Logic.Reporting;
using WUApiLib;

namespace Cegeka.Updater.Logic.Installation
{
    public class Result
    {
        public IInstallationResult InstallationResult { get; set; }
        public UpdateInstallationLog UpdateInstallationLog { get; set; }
    }
}
