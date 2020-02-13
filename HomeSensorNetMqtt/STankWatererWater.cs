using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSensorNetMqtt
{
    class STankWatererWater : ITransmitPacket
    {
        public EMessageType type;
        public UInt16 litres;

        public STankWatererWater()
        {
            type = EMessageType.TankWatererWater;
        }

        public byte[] GetBody()
        {
            byte[] msg = new byte[3];
            msg[0] = (byte)type;
            BitConverter.GetBytes(litres).CopyTo(msg, 1);
            return msg;
        }

        public void Init(dynamic msg)
        {
            litres = msg.Litres;
        }
    }
}
