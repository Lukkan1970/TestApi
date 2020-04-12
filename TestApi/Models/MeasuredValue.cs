using System;
using System.Collections.Generic;

namespace TestApi.Models
{
    public class MeasuredValue
    {
        public DateTime Date { get; set; }
        public IEnumerable<SensorTypeValue> SensorValues { get; set; }
    }
}
