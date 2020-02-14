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
            sensor = new XBeeMqtt( Properties.Settings.Default.MqttBrokerHost,
                                   Properties.Settings.Default.MqttBrokerPort,
                                   Properties.Settings.Default.SerialPortName,
                                   Properties.Settings.Default.SerialPortBaudRate);
        }

        protected override void OnStop()
        {
        }
    }
}
