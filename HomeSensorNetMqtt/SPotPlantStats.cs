using Newtonsoft.Json;
using System;

namespace HomeSensorNetMqtt
{
    [JsonObject(MemberSerialization.OptIn)]
    class SPotPlantStats : SReceivePacket
    {
        public EMessageType type;
        public Int16 uInternalTemperature;  // in 0.1C units
        public Int16 uExternalTemperature;  // in 0.1C units
        public UInt16 uVBat;  // in 0.01V units
        [JsonProperty] public double InternalTemperature;
        [JsonProperty] public double ExternalTemperature;
        [JsonProperty] public double VBat;
        [JsonProperty] public UInt16 Moisture;
        [JsonProperty] public Byte Sequence;

        public SPotPlantStats(byte[] body)
        {
            type = EMessageType.TankWatererStats;
            Sequence = body[1];
            Moisture = BitConverter.ToUInt16(body, 2);
            uVBat = BitConverter.ToUInt16(body, 4);
            uInternalTemperature = BitConverter.ToInt16(body, 6);
            uExternalTemperature = BitConverter.ToInt16(body, 8);
            VBat = (double)uVBat / 100.0;
            InternalTemperature = (double)uInternalTemperature / 10.0;
            ExternalTemperature = (double)uExternalTemperature / 10.0;
        }
    }
}