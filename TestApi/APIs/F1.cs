using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestApi.Models;
using FSharpTestApi;
using FSharp.Data;

namespace TestApi.APIs
{
    public class F1
    {
        public static async Task<string> GetValuesAsync(string deviceId, string date, string? sensorType)
        {
            var measuredValues = FSharpTestApiModule.GetMeasuredValuesAsync (deviceId, date, sensorType);
            var temp = measuredValues.ToString();

            return temp;
        }
    }
}
