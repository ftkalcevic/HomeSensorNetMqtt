using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSensorNetMqtt
{
    class STankWatererValve : ITransmitPacket
    {
        public EMessageType type;
        public bool ValveOpen;

        public STankWatererValve()
        {
            type = EMessageType.TankWatererValve;
        }

        public void Init(dynamic msg)
        {
            ValveOpen = msg.ValveOpen;
        }

        public STankWatererValve(byte[] body)
        {
            type = EMessageType.TankWatererValve;
            ValveOpen = body[1] != 0;
        }

        public string makeJSON()
        {
            string json = "{";
            json += $"\"ValveOpen\": {ValveOpen.ToString()}";
            json += "}";
            return json;
        }

        public byte[] GetBody()
        {
            byte[] msg = new byte[2];
            msg[0] = (byte)type;
            msg[1] = (byte)(ValveOpen ? 1 : 0);
            return msg;
        }
    }
}
