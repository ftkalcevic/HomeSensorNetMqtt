using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSensorNetMqtt
{
	[JsonObject(MemberSerialization.OptIn)]
	class STankWatererParameters : SReceivePacket, ITransmitPacket
	{
        public EMessageType type;
		[JsonProperty] bool AutoVent;
		[JsonProperty] bool AutoDepth;
		[JsonProperty] UInt16 ValveTimeOut;
		[JsonProperty] UInt16 TankDepth;
		[JsonProperty] UInt16 SurfaceArea;
		[JsonProperty] UInt16 SensorDistance;
		[JsonProperty] UInt16 PulsesPerLitre;
		[JsonProperty] UInt16 AutoVentTime;
		[JsonProperty] UInt16 MaxValveOpenTime;
		[JsonProperty] UInt16 MaxWater24Hours;

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
			MaxWater24Hours = BitConverter.ToUInt16(body, 17);
		}

		public STankWatererParameters()
		{
			type = EMessageType.TankWatererSetParameters;
		}

		public byte[] GetBody()
		{
			byte[] msg = new byte[19];
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
			BitConverter.GetBytes(MaxWater24Hours).CopyTo(msg, 17);
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
			MaxWater24Hours = msg.MaxWater24Hours;
		}
	}
}
