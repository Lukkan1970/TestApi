using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestApi.AzureBlobFunctions;
using TestApi.Models;

namespace TestApi.APIs
{
    public static class V1
    {
        public static async Task<DeviceMeasuredValues> GetValuesAsync(string deviceId, string date, string? sensorType)
        {
            //var temp = new SensorTypeValue { SensorType = "temperature", SensorValue = 76 };
            //var hum = new SensorTypeValue { SensorType = "humidity", SensorValue = 123 };
            //var rain = new SensorTypeValue { SensorType = "rain", SensorValue = 1 };
            //var ms = new List<SensorTypeValue>();
            //ms.Add(temp);
            //ms.Add(hum);
            //ms.Add(rain);
            //List<MeasuredValue> mv = new List<MeasuredValue>();
            //for (int i = 0; i < 10; i++)
            //{
            //    var val = new MeasuredValue { Date = DateTime.Now, SensorValues = ms };
            //    mv.Add(val);
            //}
            //DeviceMeasuredValues dev = new DeviceMeasuredValues
            //{
            //    Name = "deviceId",
            //    MeasuredValues = mv
            //};

            DeviceMeasuredValues measuredValues = await AzureBlobRetriever.GetMeasuredValuesAsync(deviceId, date, sensorType);

            return measuredValues;
        }
    }
}
