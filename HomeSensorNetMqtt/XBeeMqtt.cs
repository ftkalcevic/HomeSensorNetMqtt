using Newtonsoft.Json;
using System;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using XBeeLib;
using uPLibrary.Networking.M2Mqtt;
using System.Globalization;

namespace HomeSensorNetMqtt
{
    public class XBeeMqtt
    {
        public delegate void LogMsgEventHandler(string msg);
        public event LogMsgEventHandler LogMsgEvent;

        SerialPort port;
        XBee bee;
        MqttClient mqttClient;
        Regex exDevice;
        Regex exCmd;
        string serialPortName;
        int baudRate;
        string mqttBrokerHost;
        int mqttBrokerPort;

        public void Log(string msg)
        {
            if (LogMsgEvent != null)
                LogMsgEvent(msg);
        }

        public XBeeMqtt(string _serialPortName,int _baudRate,string _mqttBrokerHost, int _mqttBrokerPort)
        {
            serialPortName= _serialPortName;
            baudRate=_baudRate;
            mqttBrokerHost=_mqttBrokerHost;
            mqttBrokerPort=_mqttBrokerPort;

            exDevice = new Regex("/cmd/TankWaterer/([0-9a-fA-F]+)/[a-zA-Z]+", RegexOptions.Compiled);
            exCmd = new Regex("/cmd/TankWaterer/[0-9a-fA-F]+/([a-zA-Z]+)", RegexOptions.Compiled);
        }

        public void Start()
        {
            Log($"Starting XBeeMqtt serialPortName={serialPortName} baudRate={baudRate} mqttBrokerHost={mqttBrokerHost} mqttBrokerPort={mqttBrokerPort}");

            port = new SerialPort(serialPortName, baudRate);
            port.DataReceived += Port_DataReceived;
            port.Open();

            bee = new XBee();
            bee.setSerial(port);

            mqttClient = new MqttClient(mqttBrokerHost, mqttBrokerPort, false, null, null, MqttSslProtocols.None);
            string clientId = Guid.NewGuid().ToString();
            mqttClient.Connect(clientId);

            mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
            mqttClient.Subscribe(new string[] { "/cmd/TankWaterer/#" }, new byte[] { 0 });
        }
        private void MqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            try
            {
                string topic = e.Topic;
                string msgString = System.Text.Encoding.UTF8.GetString(e.Message);
                string device = "";
                string command = "";
                Match m = exDevice.Match(topic);
                if (m.Success)
                    device = m.Groups[1].Value;
                else
                    throw new ApplicationException($"Failed to extract device from topic '{topic}'");
                m = exCmd.Match(topic);
                if (m.Success)
                    command = m.Groups[1].Value;
                else
                    throw new ApplicationException($"Failed to extract command from topic '{topic}'");

                Log($"device={device} cmd={command} topic={topic} msg={msgString}");

                UInt64 address = 0;
                if (UInt64.TryParse(device, System.Globalization.NumberStyles.HexNumber, CultureInfo.CurrentCulture, out address))
                {
                }

                ITransmitPacket pkt = null;
                switch (command.ToLower())
                {
                    case "setvalve":
                        pkt = new STankWatererValve();
                        break;
                    case "water":
                        pkt = new STankWatererWater();
                        break;
                    case "setparameters":
                        pkt = new STankWatererParameters();
                        break;
                    case "query":
                        pkt = new STankWatererQueryStats();
                        break;
                    case "queryparameters":
                        pkt = new STankWatererQueryParameters();
                        break;
                    case "resetcounters":
                        pkt = new STankWatererResetCounters();
                        break;
                    case "settemperature":
                        pkt = new STankWatererSetTemperature();
                        break;
                }
                if (pkt != null)
                {
                    dynamic data = JsonConvert.DeserializeObject(msgString);
                    pkt.Init(data);
                    byte[] body = pkt.GetBody();
                    SendXBeeMessage(address, body);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void SendXBeeMessage(UInt64 address, byte[] body)
        {
            XBeeAddress64 addr = new XBeeAddress64(address);
            ZBTxRequest tx = new ZBTxRequest(addr, body, (byte)body.Length);
            bee.send(tx);
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                while (true)
                {
                    bee.readPacket();
                    if (bee.getResponse().isAvailable())
                    {
                        // Process packet.
                        //Log($"Got a packet {bee.getResponse().getFrameDataLength()} {bee.getResponse().getPacketLength()} ");
                        ProcessMessage(bee.getResponse());
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void ProcessMessage(XBeeResponse msg)
        {
            if (msg.getApiId() == XBee.ZB_RX_RESPONSE)
            {
                ZBRxResponse response = new ZBRxResponse();
                msg.getZBRxResponse(response);
                byte[] body = response.getData();

                string topic = "";
                string payload = "";
                SReceivePacket pkt = null;
                switch ((EMessageType)body[0])
                {
                    case EMessageType.TankWatererStats:
                        pkt = new STankWatererStats(body);
                        topic = $"/tele/TankWaterer/{response.getRemoteAddress64().get().ToString("X16")}/info";
                        break;
                    case EMessageType.TankWatererManual:
                        pkt = new STankWatererValve(body);
                        topic = $"/tele/TankWaterer/{response.getRemoteAddress64().get().ToString("X16")}/manual";
                        break;
                    case EMessageType.TankWatererParameters:
                        pkt = new STankWatererParameters(body);
                        topic = $"/tele/TankWaterer/{response.getRemoteAddress64().get().ToString("X16")}/parameters";
                        break;
                }
                payload = JsonConvert.SerializeObject(pkt);
                Log(topic + " " + payload);
                mqttClient.Publish(topic, Encoding.ASCII.GetBytes(payload));
            }
            else if (msg.getApiId() == XBee.ZB_TX_STATUS_RESPONSE)
            {
                ZBTxStatusResponse response = new ZBTxStatusResponse();
                msg.getZBTxStatusResponse(response);

                Log("TxResponse " + (response.isSuccess() ? "OK" : $"Error: {response.getErrorCode()}"));
            }
            else
            {
                Log($"Got Packet ApiId={msg.getApiId()}");
            }
        }

        public void send(UInt64 deviceAddress, string topic, string payload)
        {
            string body = topic + "," + payload;
            byte[] buf = Encoding.ASCII.GetBytes(body);
            var msg = new XBeeLib.ZBTxRequest(new XBeeAddress64(deviceAddress), buf, (byte)buf.Length);
        }
    }
}
