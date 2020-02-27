using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSensorNetMqtt
{
    public enum EMessageType: byte
    {
        TankWatererStats = 1,
        TankWatererManual = 2,
        TankWatererParameters = 3,

        PotPlantStats = 10,

        TankWatererQueryStats = 50,
        TankWatererQueryParameters = 51,
        TankWatererResetCounters = 52,
        TankWatererValve = 53,
        TankWatererWater = 54,
        TankWatererSetParameters = 55,
        TankWatererSetTemperature = 56
    }
}
