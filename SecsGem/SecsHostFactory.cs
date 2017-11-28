using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsHostFactory
    {
        public SecsHostFactory() { 
            
        }

        public SecsHost Create(EquipmentModel eqModel)
        {
            GemOption opt = eqModel.Connection;

            SecsHost host = null;

            if (opt.Protocol == GemProtocol.HSMS)
            {
                host = new HsmsHost(opt.HsmsParameters);
            }
            else if (opt.Protocol == GemProtocol.SECS_I)
            {
                host = new SecsIHost(opt.SecsIParameters);
            }

            host.DeviceId = opt.DeviceId;

            return host;
        }
    }
}
