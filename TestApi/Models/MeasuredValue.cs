using System;
using System.Collections.Generic;

namespace TestApi.Models
{
    public class MeasuredValue
    {
        public string Date { get; set; }
        public IEnumerable<SensorTypeValue> SensorValues { get; set; }
    }
}
