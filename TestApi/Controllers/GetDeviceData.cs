using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestApi.APIs;
using TestApi.Models;

namespace TestApi.Controllers
{
    [ApiController]
    [Route("api/{apiVersion}/device/{deviceId}/data/{date}/{sensorType?}")]
    public class GetDeviceData : ControllerBase
    {
        [HttpGet]
        public async System.Threading.Tasks.Task<DeviceMeasuredValues> GetAsync(string apiVersion, string deviceId, string date, string? sensorType)
        {
            DeviceMeasuredValues deviceMeasuredValues = apiVersion.ToUpper() switch
            {
                "V1" => await V1.GetValuesAsync(deviceId, date, sensorType),
                _ => new DeviceMeasuredValues { Name = "Wrong API version"}
            };

            return deviceMeasuredValues;
        }
       
    }
}
