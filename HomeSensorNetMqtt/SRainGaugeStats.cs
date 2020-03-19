using Newtonsoft.Json;
using System;

namespace HomeSensorNetMqtt
{
    [JsonObject(MemberSerialization.OptIn)]
    class SRainGaugeStats : SReceivePacket
    {
        public EMessageType type;
        [JsonProperty] public Byte Sequence;
        [JsonProperty] public double millimeters;
        [JsonProperty] public double temperature;
        [JsonProperty] public double humidity;
        [JsonProperty] public double vbat;
        [JsonProperty] public double vsolar;

        /*
            SHomeSensorMsgHdr hdr;
            uint8_t sequence;
            uint32_t millilitres;
            int16_t temperature;
            uint16_t humidity;
            uint16_t vbat;
            uint16_t vsolar;

         */
        public SRainGaugeStats(byte[] body)
        {
            type = EMessageType.RainGaugeStats;
            Sequence = body[1];
            UInt32 imillimeters = BitConverter.ToUInt32(body, 2);       // 0.01 ml units
            Int16 itemperature = BitConverter.ToInt16(body, 6);         // 0.1 C units
            UInt16 ihumidity = BitConverter.ToUInt16(body, 8);          // 0.1 %
            UInt16 ivbat = BitConverter.ToUInt16(body, 10);             // mV
            UInt16 ivsolar = BitConverter.ToUInt16(body, 12);           // mV

            millimeters = (double)imillimeters / 100.0;
            temperature = (double)itemperature / 10.0;
            humidity = (double)ihumidity / 10.0;
            vbat = (double)ivbat / 1000.0;
            vsolar = (double)ivsolar / 1000.0;
        }
    }
}