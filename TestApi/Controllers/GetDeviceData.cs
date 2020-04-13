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
        private readonly ILogger<GetDeviceData> _logger;

        public GetDeviceData(ILogger<GetDeviceData> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public DeviceMeasuredValues Get(string apiVersion, string deviceId, string date, string? sensorType)
        {
            DeviceMeasuredValues deviceMeasuredValues = apiVersion.ToUpper() switch
            {
                "V1" => V1.GetValues(deviceId, date, sensorType),
                _ => new DeviceMeasuredValues { Name = "Wrong API version"}
            };

            return deviceMeasuredValues;
        }
       
    }
}
