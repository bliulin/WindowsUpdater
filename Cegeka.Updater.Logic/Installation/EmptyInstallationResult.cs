using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WUApiLib;

namespace Cegeka.Updater.Logic.Installation
{
    class EmptyInstallationResult : IInstallationResult
    {
        public IUpdateInstallationResult GetUpdateResult(int updateIndex)
        {
            return null;
        }

        public int HResult { get; set; }

        public bool RebootRequired { get; set; }

        public OperationResultCode ResultCode { get; set; }
    }
}
