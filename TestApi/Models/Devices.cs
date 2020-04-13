using Microsoft.AspNetCore.Routing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApi.Models
{
    public class Devices
    {
        public string DeviceId { get; set; }
        public IEnumerable<string> SensorTypes { get; set; }
    }
}
