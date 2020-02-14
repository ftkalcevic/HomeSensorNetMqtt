using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSensorNetMqtt
{
    [JsonObject(MemberSerialization.OptIn)]
    class SReceivePacket
    {
        [JsonProperty]
        public DateTime timestamp;

        public SReceivePacket()
        {
            timestamp = DateTime.Now;
        }

    }
}
