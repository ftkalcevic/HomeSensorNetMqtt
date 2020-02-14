# HomeSensorNetMqtt

This is a replacement for my tank watering monitor/sensor and pot plant sensor.  When complete, this will
use a ZigBee mesh network with one controller, rather than a collection of point to point pairs.

* HomeSersnorNetMqtt - main functionality in a .net library
* HomeSersnorNetMqttTest - command line test function.  The final version will be a Windows Service.
* XBee - C# port of XBee-Arduino
* HomeSernsorNetCli - an attempt to import the sensor binary messages into c# automatically.  This didn't work - I got close but failed with field packing.