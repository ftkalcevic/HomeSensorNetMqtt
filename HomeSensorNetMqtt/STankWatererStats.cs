using System;

namespace HomeSensorNetMqtt
{
    public class STankWatererStats
    {
        public EMessageType type;
        public bool HighWaterMark;
        public bool ValveOpen;
        public UInt32 TankFlow;
        public UInt32 TankOverflow;
        public UInt16 Moisture1;
        public UInt16 Moisture2;
        public UInt16 TankVolume;
        public UInt16 Temperature;  // in 0.1C units

        public STankWatererStats(byte[] body)
        {
            type = EMessageType.TankWatererStats;
            HighWaterMark = (body[1] & 0x01) != 0;
            ValveOpen = (body[1] & 0x02) != 0;
            TankFlow = BitConverter.ToUInt32(body,2);
            TankOverflow = BitConverter.ToUInt32(body, 6);
            Moisture1 = BitConverter.ToUInt16(body, 10);
            Moisture2 = BitConverter.ToUInt16(body, 12);
            TankVolume = BitConverter.ToUInt16(body, 14);
            Temperature = BitConverter.ToUInt16(body, 16);
        }

        public string makeJSON()
        {
            string json = "{";
            json += $"\"HighWaterMark\": {HighWaterMark.ToString()},";
            json += $"\"ValveOpen\": {ValveOpen.ToString()},";
            json += $"\"TankFlow\": {(TankFlow / 10).ToString()}.{(TankFlow % 10).ToString()},";
            json += $"\"TankOverflow\": {(TankOverflow / 10).ToString()}.{(TankOverflow % 10).ToString()},";
            json += $"\"Moisture1\": {Moisture1.ToString()},";
            json += $"\"Moisture2\": {Moisture2.ToString()},";
            json += $"\"TankVolume\": {TankVolume.ToString()},";
            json += $"\"Temperature\": {(Temperature / 10).ToString()}.{(Temperature % 10).ToString()}";
            json += "}";
            return json;
        }
    }
}