using System;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using NUnit;
using NUnit.Framework;
using SimpleInjector;

namespace DSImager.Tests
{
    [TestFixture]
    public class CameraServiceTests
    {

        private ICameraService _cameraService;
        private Container _container;

        [OneTimeSetUp]
        public void Init()
        {
            _container = Bootstrapper.Container;
            _cameraService = _container.GetInstance<ICameraService>();
            var initialized = _cameraService.Initialize("ASCOM.Simulator.Camera");
            Assert.True(initialized);
        }

        [Test]
        public async Task TakeSuccessfulExposure()
        {
            _cameraService.OnExposureProgressChanged += delegate(double duration, double exposureDuration)
            {
                Console.WriteLine(duration);
            };

            _cameraService.OnExposureCompleted += delegate(bool successful, Exposure exposure)
            {
                Console.WriteLine("Exposure successful");
                Console.WriteLine("Width: " + exposure.Width + ", Height: " + exposure.Height);
            };

            var exposureOk = await _cameraService.StartExposure(2.0, false);
            Assert.True(exposureOk);

        }

        [Test]
        public async Task TakeAndStopExposure()
        {
            _cameraService.OnExposureProgressChanged += delegate(double duration, double exposureDuration)
            {
                Console.WriteLine(duration);
            };

            _cameraService.OnExposureCompleted += delegate(bool successful, Exposure exposure)
            {
                Console.WriteLine("Exposure successful");
                Console.WriteLine("Width: " + exposure.Width + ", Height: " + exposure.Height);
                Console.WriteLine("Exposure time: " + exposure.MetaData.ExposureTime);
            };

            var exposureTask = _cameraService.StartExposure(5.0, false);
            await Task.Delay(1000);
            _cameraService.StopExposure();

            var result = await exposureTask;
            Assert.True(result);

        }
    }
}
