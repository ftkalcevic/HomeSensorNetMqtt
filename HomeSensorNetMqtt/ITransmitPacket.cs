
namespace HomeSensorNetMqtt
{
    interface ITransmitPacket
    {
        void Init(dynamic msg);
        byte[] GetBody();
    }
}
