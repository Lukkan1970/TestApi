using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestApi.APIs;
using TestApi.Models;

#nullable enable

namespace TestApi.Controllers
{
    [ApiController]
    [Route("api/{apiVersion}/device/{deviceId}/data/{date}/{sensorType?}")]
    public class GetDeviceData : ControllerBase
    {
        [HttpGet]
        public async System.Threading.Tasks.Task<string?> GetAsync(string apiVersion, string deviceId, string date, string? sensorType)
        {
            var retVal = new DeviceMeasuredValues { Name = "Wrong API version" };
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(retVal);

            string? deviceMeasuredValues = apiVersion.ToUpper() switch
            {
                "V1" => await V1.GetValuesAsync(deviceId, date, sensorType),
                "F1"=> await F1.GetValuesAsync(deviceId, date, sensorType),
                _ => result
            };
            return deviceMeasuredValues;
        }
       
    }
}
