using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        public IEnumerable<DummyClass> Get(string apiVersion, string deviceId, string date, string? sensorType)
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new DummyClass
            {
                id = index,
                api = apiVersion
            })
            .ToArray();
        }
        public class DummyClass
        {
            public int id { get; set; }
            public string api { get; set; }

        }
    }
}
