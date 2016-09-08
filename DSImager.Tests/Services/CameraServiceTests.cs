using System;
using System.Threading.Tasks;
using ASCOM.DeviceInterface;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using DSImager.Core.Services;
using Moq;
using NUnit;
using NUnit.Framework;
using SimpleInjector;

namespace DSImager.Tests.Services
{
    [TestFixture]
    public class CameraServiceTests
    {
        
        [OneTimeSetUp]
        public void Init()
        {
        }

        private Mock<ILogService> MockLogService()
        {
            var service = new Mock<ILogService>();
            service.Setup(x => x.LogMessage(It.IsAny<LogMessage>()))
                .Callback<LogMessage>((m) => Console.WriteLine(m.Message));
            return service;
        }

        [Test]
        public async Task TestTakeSingleExposure()
        {
            Console.WriteLine("Running TakeSuccessfulExposure");

            var moqApp = new Mock<IApplication>().SetupAllProperties();
            var moqLogService = MockLogService();

            var moqCamera = new Mock<ICameraV2>();
            moqCamera.SetupGet(x => x.Connected).Returns(true);
            moqCamera.SetupGet(x => x.CameraState).Returns(CameraStates.cameraIdle);
            moqCamera.SetupGet(x => x.MaxADU).Returns(1);
            moqCamera.Setup(x => x.StartExposure(It.IsAny<double>(), It.IsAny<bool>())).Callback(() =>
            {
                moqCamera.SetupGet(x => x.CameraState).Returns(CameraStates.cameraExposing);
                Task.Delay(500).ContinueWith((t) =>
                {
                    moqCamera.SetupGet(x => x.CameraState).Returns(CameraStates.cameraIdle);
                    moqCamera.SetupGet(x => x.ImageReady).Returns(true);
                });
            });
            moqCamera.SetupGet(x => x.ImageArray).Returns(new int[,] {{1, 1}, {1, 1}});

            var moqCameraProvider = new Mock<ICameraProvider>();
            moqCameraProvider.Setup(x => x.ChooseCameraDeviceId()).Returns("camera");
            moqCameraProvider.Setup(x => x.GetCamera(It.IsAny<string>())).Returns(moqCamera.Object);
            
            var cameraService = new CameraService(moqLogService.Object, moqCameraProvider.Object, moqApp.Object);
            bool initialized = cameraService.Initialize("camera");
            Assert.True(initialized);

            cameraService.OnExposureProgressChanged += delegate(double duration, double exposureDuration, ExposurePhase phase)
            {
                Console.WriteLine(duration);
            };

            cameraService.OnExposureCompleted += delegate(bool successful, Exposure exposure)
            {
                Console.WriteLine("Exposure successful");
                Console.WriteLine("Width: " + exposure.Width + ", Height: " + exposure.Height);
            };

            var exposureOk = await cameraService.TakeExposure(2.0, false);

            moqCamera.Verify(x => x.StartExposure(It.IsAny<double>(), It.IsAny<bool>()), Times.Once);
            Assert.True(exposureOk);


        }
        
    }
}
