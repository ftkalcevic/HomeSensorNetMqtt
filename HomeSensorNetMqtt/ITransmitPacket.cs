using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSensorNetMqtt
{
    interface ITransmitPacket
    {
        void Init(dynamic msg);
        byte[] GetBody();
    }
}
