using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using HomeSensorNetMqtt;

namespace HomeSensorNetMqttService
{
    public partial class HomeSensorService : ServiceBase
    {
        XBeeMqtt sensor;
        public HomeSensorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                sensor = new XBeeMqtt(Properties.Settings.Default.SerialPortName,
                                      Properties.Settings.Default.SerialPortBaudRate,
                                      Properties.Settings.Default.MqttBrokerHost,
                                      Properties.Settings.Default.MqttBrokerPort
                                      );
                sensor.LogMsgEvent += Sensor_LogMsgEvent;
                sensor.Start();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        private void Sensor_LogMsgEvent(string msg)
        {
            Trace.TraceInformation(msg);
        }

        protected override void OnStop()
        {
        }
    }
}
