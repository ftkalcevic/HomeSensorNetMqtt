using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSensorNetMqtt
{
    class STankWatererSetTemperature : ITransmitPacket
    {
        public EMessageType type;
        public UInt16 Temperature;      // in 0.1C units

        public STankWatererSetTemperature()
        {
            type = EMessageType.TankWatererSetTemperature;
        }

        public byte[] GetBody()
        {
            byte[] buf = new byte[3];
            buf[0] = (byte)type;
            BitConverter.GetBytes(Temperature).CopyTo(buf, 1);
            return buf;
        }

        public void Init(dynamic msg)
        {
            double temp = msg.Temperature;
            Temperature = (UInt16)Math.Round(temp * 10.0);
        }
    }
}
