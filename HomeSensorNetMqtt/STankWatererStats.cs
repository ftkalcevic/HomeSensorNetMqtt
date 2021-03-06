﻿using Newtonsoft.Json;
using System;

namespace HomeSensorNetMqtt
{
    [JsonObject(MemberSerialization.OptIn)]
    class STankWatererStats: SReceivePacket
    {
        public EMessageType type;
        public UInt32 uTankFlow;
        public UInt32 uTankOverflow;
        public UInt32 uWater24Hours;
        public UInt16 uTemperature;  // in 0.1C units
        [JsonProperty] public bool HighWaterMark;
        [JsonProperty] public bool ValveOpen;
        [JsonProperty] public UInt16 Moisture1;
        [JsonProperty] public UInt16 Moisture2;
        [JsonProperty] public UInt16 TankVolume;
        [JsonProperty] public double TankFlow;
        [JsonProperty] public double TankOverflow;
        [JsonProperty] public double Water24Hours;
        [JsonProperty] public double Temperature;  // in 0.1C units

        public STankWatererStats(byte[] body)
        {
            type = EMessageType.TankWatererStats;
            HighWaterMark = (body[1] & 0x01) != 0;
            ValveOpen = (body[1] & 0x02) != 0;
            uTankFlow = BitConverter.ToUInt32(body,2);
            uTankOverflow = BitConverter.ToUInt32(body, 6);
            uWater24Hours = BitConverter.ToUInt16(body, 10);
            Moisture1 = BitConverter.ToUInt16(body, 14);
            Moisture2 = BitConverter.ToUInt16(body, 16);
            TankVolume = BitConverter.ToUInt16(body, 18);
            uTemperature = BitConverter.ToUInt16(body, 20);
            TankFlow = (double)uTankFlow / 10.0;
            TankOverflow = (double)uTankOverflow / 10.0;
            Temperature = (double)uTemperature / 10.0;
            Water24Hours = (double)uWater24Hours / 10.0;
        }
    }
}