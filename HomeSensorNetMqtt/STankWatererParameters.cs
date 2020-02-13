using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSensorNetMqtt
{
    class STankWatererParameters: ITransmitPacket
    {
        public EMessageType type;
		bool AutoVent;
		bool AutoDepth;
		UInt16 ValveTimeOut;
		UInt16 TankDepth;
		UInt16 SurfaceArea;
		UInt16 SensorDistance;
		UInt16 PulsesPerLitre;
		UInt16 AutoVentTime;
		UInt16 MaxValveOpenTime;

		public STankWatererParameters(byte[] body) 
        {
            type = EMessageType.TankWatererParameters;
			AutoVent = body[1] != 0;
			AutoDepth = body[2] != 0;
			ValveTimeOut = BitConverter.ToUInt16(body,3);
			TankDepth = BitConverter.ToUInt16(body, 5);
			SurfaceArea = BitConverter.ToUInt16(body, 7);
			SensorDistance = BitConverter.ToUInt16(body, 9);
			PulsesPerLitre = BitConverter.ToUInt16(body, 11);
			AutoVentTime = BitConverter.ToUInt16(body, 13);
			MaxValveOpenTime = BitConverter.ToUInt16(body, 15);
		}

		public STankWatererParameters()
		{
			type = EMessageType.TankWatererSetParameters;
		}

		public byte[] GetBody()
		{
			byte[] msg = new byte[17];
			msg[0] = (byte)type;
			msg[1] = (byte)(AutoVent ? 1 : 0);
			msg[2] = (byte)(AutoDepth ? 1 : 0);
			BitConverter.GetBytes(ValveTimeOut).CopyTo(msg, 3);
			BitConverter.GetBytes(TankDepth).CopyTo(msg, 5);
			BitConverter.GetBytes(SurfaceArea).CopyTo(msg, 7);
			BitConverter.GetBytes(SensorDistance).CopyTo(msg, 9);
			BitConverter.GetBytes(PulsesPerLitre).CopyTo(msg, 11);
			BitConverter.GetBytes(AutoVentTime).CopyTo(msg, 13);
			BitConverter.GetBytes(MaxValveOpenTime).CopyTo(msg, 15);
			return msg;
		}

		public void Init(dynamic msg)
		{
			AutoVent = msg.AutoVent;
			AutoDepth = msg.AutoDepth;
			ValveTimeOut = msg.ValveTimeOut;
			TankDepth = msg.TankDepth;
			SurfaceArea = msg.SurfaceArea;
			SensorDistance = msg.SensorDistance;
			PulsesPerLitre = msg.PulsesPerLitre;
			AutoVentTime = msg.AutoVentTime;
			MaxValveOpenTime = msg.MaxValveOpenTime;
		}

		public string makeJSON()
		{
			string json = "{";
			json += $"\"AutoVent\": {AutoVent.ToString()},";
			json += $"\"AutoDepth\": {AutoDepth.ToString()},";
			json += $"\"ValveTimeOut\": {ValveTimeOut.ToString()},";
			json += $"\"TankDepth\": {TankDepth.ToString()},";
			json += $"\"SurfaceArea\": {SurfaceArea.ToString()},";
			json += $"\"SensorDistance\": {SensorDistance.ToString()},";
			json += $"\"PulsesPerLitre\": {PulsesPerLitre.ToString()},";
			json += $"\"AutoVentTime\": {AutoVentTime.ToString()},";
			json += $"\"MaxValveOpenTime\": {MaxValveOpenTime.ToString()}";
			json += "}";
			return json;
		}
	}
}
