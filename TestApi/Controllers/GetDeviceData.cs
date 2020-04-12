using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestApi.Models;

namespace TestApi.Controllers
{
    [ApiController]
    [Route("api/{apiVersion}/device/{deviceId}/data/{date}/{sensorType?}")]
    public class GetDeviceData : ControllerBase
    {
        private readonly ILogger<GetDeviceData> _logger;

        public GetDeviceData(ILogger<GetDeviceData> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public DeviceMeasuredValues Get(string apiVersion, string deviceId, string date, string? sensorType)
        {
            var temp = new SensorTypeValue { SensorType = "temperature", SensorValue = 76 };
            var hum = new SensorTypeValue { SensorType = "humidity", SensorValue = 123 };
            var rain = new SensorTypeValue { SensorType = "rain", SensorValue = 1 };
            var ms = new List<SensorTypeValue>();
            ms.Add(temp);
            ms.Add(hum);
            ms.Add(rain);
            List<MeasuredValue> mv = new List<MeasuredValue>();
            for (int i = 0; i < 10; i++)
            { 
                var val = new MeasuredValue { Date = DateTime.Now, SensorValues = ms };
                mv.Add(val);
            }
            DeviceMeasuredValues dev = new DeviceMeasuredValues
            {
                Name = "Device 1",
                MeasuredValues = mv
            };

                
            return dev;
        }
       
    }
}
