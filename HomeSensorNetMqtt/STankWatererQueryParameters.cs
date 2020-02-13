using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSensorNetMqtt
{
    class STankWatererQueryParameters : ITransmitPacket
    {
        public EMessageType type;

        public STankWatererQueryParameters()
        {
            type = EMessageType.TankWatererQueryParameters;
        }

        public byte[] GetBody()
        {
            byte[] buf = new byte[1] { (byte)type };
            return buf;
        }

        public void Init(dynamic msg)
        {
        }
    }
}
