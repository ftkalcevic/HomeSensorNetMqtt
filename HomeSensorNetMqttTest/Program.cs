﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeSensorNetMqtt;

namespace HomeSensorNetMqttTest
{
    class Program
    {
        const UInt64 deviceId = 0x0013A2004127CE89;

        static void Main(string[] args)
        {
            XBeeMqtt sensor = new XBeeMqtt("COM22", 115200, "server", 1883);
            sensor.LogMsgEvent += Sensor_LogMsgEvent;
            sensor.Start();

            //string host = deviceId.ToString("X16");
            //sensor.send(deviceId, "/cmd/" + host + "/Query", null);

            System.Console.ReadLine();
        }

        private static void Sensor_LogMsgEvent(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
