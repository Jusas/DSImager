using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Models
{
    public class CameraCapabilities
    {
        // capabilities: camera size, bin size, can abort exposure, can asymmetricbin, can fastreadout, can getcoolerpower, can pulseguide,
        // can setccdtemp, can stopexposure, has shutter, has adjustable gain

        public int ResolutionX { get; set; }
        public int ResolutionY { get; set; }
        public string Resolution { get; set; }
        public int MaxBinX { get; set; }
        public int MaxBinY { get; set; }
        public bool HasShutter { get; set; }
        public bool HasAdjustableGain { get; set; }
        public string MinGain { get; set; }
        public string MaxGain { get; set; }
        public bool CanStopExposure { get; set; }
        public bool CanAbortExposure { get; set; }
        public bool CanAsymmetricBin { get; set; }
        public bool CanFastReadOut { get; set; }
        public bool CanSetCcdTemperature { get; set; }
        public bool CanGetCoolerPower { get; set; }
        public bool CanPulseGuide { get; set; }
        public double MinExposure { get; set; }
        public double MaxExposure { get; set; }
        public int MaxAdu { get; set; }
        public double ElectronsPerAdu { get; set; }
        public double PixelSizeX { get; set; }
        public double PixelSizeY { get; set; }
    }
}

