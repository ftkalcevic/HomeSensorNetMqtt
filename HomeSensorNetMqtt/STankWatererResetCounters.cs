using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSensorNetMqtt
{
    class STankWatererResetCounters : ITransmitPacket
    {
        public EMessageType type;

        public STankWatererResetCounters()
        {
            type = EMessageType.TankWatererResetCounters;
        }

        public byte[] GetBody()
        {
            throw new NotImplementedException();
        }

        public void Init(dynamic msg)
        {
            throw new NotImplementedException();
        }
    }
}
