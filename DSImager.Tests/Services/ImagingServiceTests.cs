using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASCOM.DeviceInterface;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using DSImager.Core.Services;
using Moq;
using NUnit.Framework;
using SimpleInjector;

namespace DSImager.Tests.Services
{
    [TestFixture]
    public class ImagingServiceTests
    {

        [OneTimeSetUp]
        public void Init()
        {
        }

        private Mock<ICameraService> MockCameraService()
        {
            var service = new Mock<ICameraService>();
            var mockCam = new Mock<ICameraV2>();
            mockCam.SetupAllProperties();
            mockCam.Setup(x => x.CameraXSize).Returns(800);
            mockCam.Setup(x => x.CameraYSize).Returns(600);
            service.Setup(x => x.Camera).Returns(mockCam.Object);
            return service;
        }

        private Mock<IImageIoService> MockImageIoService()
        {
            var service = new Mock<IImageIoService>();
            service.Setup(x => x.WritableFileFormats)
                .Returns(new List<ImageFileFormat>()
                {
                    new ImageFileFormat("FITS", "FITS", "fits", "FITS")
                });            
            return service;
        }

        private Mock<ILogService> MockLogService()
        {
            var service = new Mock<ILogService>();
            service.Setup(x => x.LogMessage(It.IsAny<LogMessage>()))
                .Callback<LogMessage>((m) => Console.WriteLine(m.Message));
            return service;
        }

        [Test]
        public async Task TestImagingServiceSequence()
        {
            var imageIoService = MockImageIoService();
            var cameraService = MockCameraService();
            var logService = MockLogService();
            var systemEnv = new Mock<ISystemEnvironment>();
            systemEnv.SetupGet(x => x.UserPicturesDirectory).Returns(Path.GetTempPath());
            systemEnv.SetupGet(x => x.UserHomeDirectory).Returns(Path.GetTempPath());
            systemEnv.SetupGet(x => x.TemporaryFilesDirectory).Returns(Path.GetTempPath());

            cameraService.Setup(x => x.TakeExposure(It.IsAny<double>(), It.IsAny<bool>()))                
                .ReturnsAsync(true)
                .Raises(x => x.OnExposureCompleted += null, true, new Exposure(800, 600, new int[0], 1, false));

            var writerMoq = new Mock<IImageWriter>();
            writerMoq.Setup(x => x.Save(It.IsAny<Exposure>(), It.IsAny<string>()));
            writerMoq.Setup(x => x.Save(It.IsAny<Exposure>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()));
            imageIoService.Setup(x => x.GetImageWriter(It.IsAny<ImageFileFormat>())).Returns(writerMoq.Object);

            var imagingService = new ImagingService(cameraService.Object, logService.Object, imageIoService.Object, systemEnv.Object);
            
            var session = new ImagingSession()
            {
                Name = "Test Session",
                SaveOutput = true,
                RepeatTimes = 2,
                PauseAfterEachRepeat = false,
                PauseAfterEachSequence = false
            };
            
            var sequence1 = new ImageSequence()
            {
                BinXY = 4,
                ExposureDuration = 90,
                Extension = "test",
                FileFormat = imageIoService.Object.WritableFileFormats.First().Id,
                Name = "Sequence 1",
                NumExposures = 2
            };
            var sequence2 = new ImageSequence()
            {
                BinXY = 1,
                ExposureDuration = 10,
                Extension = "test",
                FileFormat = imageIoService.Object.WritableFileFormats.First().Id,
                Name = "Sequence 2",
                NumExposures = 2
            };

            session.ImageSequences.Add(sequence1);
            session.ImageSequences.Add(sequence2);

            imagingService.OnImagingSessionStarted += imagingSession =>
            {
                Console.WriteLine(@"Imaging session started");
            };

            imagingService.OnImagingComplete += (successful, exposure) =>
            {
                Console.WriteLine(@"Single exposure imaging complete, success: " + successful);
                Console.WriteLine(@"Exposure filename: " + session.GenerateFilename(imagingService.CurrentImageSequence));
            };

            imagingService.OnImagingSessionCompleted += (imagingSession, successfully, userCanceled) =>
            {
                Console.WriteLine(@"Imaging session completed, success: " + successfully + @", was user canceled: " +
                                  userCanceled);
            };

            await imagingService.RunImagingSession(session);

            // 2 x 2 x 2 images, verify.
            writerMoq.Verify(x => x.Save(It.IsAny<Exposure>(), It.IsAny<string>()), Times.Exactly(8));
            
            // Asserts?

        }
    }
}
