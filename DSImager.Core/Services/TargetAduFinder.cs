using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;

namespace DSImager.Core.Services
{
    public class TargetAduFinder : ITargetAduFinder
    {
        private ICameraService _cameraService;

        public event FindExposureTakenHandler OnFindExposureTaken;
        private bool _abortFlag = false;
        private bool _isRunning = false;


        public TargetAduFinder(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public void CancelFindOperation()
        {
            if (_cameraService.IsExposuring && _isRunning)
            {
                _abortFlag = true;
                _cameraService.StopOrAbortExposure();
            }
        }

        public async Task<double> FindExposureValue(int targetAdu, double maxExposure)
        {

            _isRunning = true;

            double estimatedExpTime = Math.Max(0.1, 0.1 * maxExposure);

            if (maxExposure > _cameraService.Camera.ExposureMax)
            {
                _isRunning = false;
                throw new Exception("Exposure value too high for the camera");
            }
            
            bool ok = await _cameraService.TakeExposure(estimatedExpTime, false);
            if (!ok)
            {
                _isRunning = false;
                throw new OperationCanceledException("Exposure was not completed successfully");
            }

            int maxTries = 5;
            int tries = 0;
            
            do
            {
                if(_abortFlag)
                    throw new Exception("Operation was canceled");

                var exposure = _cameraService.LastExposure;
                var max = exposure.Histogram.Values.Max();
                var spike = exposure.Histogram.Where(x => x.Value == max).First().Key;

                OnFindExposureTaken?.Invoke(estimatedExpTime, spike);

                if (Math.Abs(targetAdu - spike) < 0.05*targetAdu)
                {
                    return estimatedExpTime;
                }
                
                double adusPerSec = spike/estimatedExpTime;

                estimatedExpTime = targetAdu/adusPerSec;
                if (estimatedExpTime > maxExposure)
                {
                    throw new Exception(
                        $"The exposure time required is estimated to be {estimatedExpTime:F}, which is larger than given maximum exposure value");
                }
                if (estimatedExpTime > _cameraService.Camera.ExposureMax)
                {
                    throw new Exception("The exposure time required is estimated to be over the max exposure value of the camera");
                }

                ok = await _cameraService.TakeExposure(estimatedExpTime, false);
                if (!ok)
                    throw new Exception("Exposure was not completed successfully");

                tries++;
            } while (tries < maxTries);

            throw new Exception("Could not find suitable exposure value to meet the target ADU");
        }
        
    }
}
