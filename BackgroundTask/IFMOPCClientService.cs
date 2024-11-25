using Middleware.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.BackgroundTask
{
    public class IFMOPCClientService
    {
        public readonly ModeConfiguration _modeConfiguration = new ModeConfiguration();
        public IFMOPCClientService(ModeConfiguration modeConfiguration)
        {
            _modeConfiguration = modeConfiguration;
        }


    }
}
