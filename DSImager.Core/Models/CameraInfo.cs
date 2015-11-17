using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Models
{
    public class CameraInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SensorName { get; set; }
        public string SensorType { get; set; }
        public string DriverInfo { get; set; }
        public string DriverVersion { get; set; }
    }
}
