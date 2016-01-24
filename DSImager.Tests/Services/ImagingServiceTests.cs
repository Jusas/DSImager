using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using NUnit.Framework;
using SimpleInjector;

namespace DSImager.Tests.Services
{
    [TestFixture]
    public class ImagingServiceTests
    {
        private IImagingService _imagingService;
        private ICameraService _cameraService;
        private Container _container;

        [OneTimeSetUp]
        public void Init()
        {
            _container = Bootstrapper.Container;
            _imagingService = _container.GetInstance<IImagingService>();
            _cameraService = _container.GetInstance<ICameraService>();
            var initialized = _cameraService.Initialize("ASCOM.Simulator.Camera");            
            Assert.True(initialized);
        }

        [Test]
        public async Task RunSessionWithSingleSequence()
        {
            ImagingSession session = new ImagingSession()
            {
                Name = "Test Session"
            };

            var sequence = new ImageSequence()
            {
                BinXY = 1,
                ExposureDuration = 1.0,
                Extension = "test",
                Format = ImageFormat.Tiff,
                Name = "Sequence 1",
                NumExposures = 3
            };

            session.ImageSequences.Add(sequence);

            _imagingService.OnImagingSessionStarted += imagingSession =>
            {
                Console.WriteLine(@"Imaging session started");
            };

            _imagingService.OnImagingComplete += (successful, exposure) =>
            {
                Console.WriteLine(@"Single exposure imaging complete, success: " + successful);
                Console.WriteLine(@"Exposure filename: " + session.GenerateFilename(_imagingService.CurrentImageSequence));
            };

            _imagingService.OnImagingSessionCompleted += (imagingSession, successfully, userCanceled) =>
            {
                Console.WriteLine(@"Imaging session completed, success: " + successfully + @", was user canceled: " +
                                  userCanceled);
            };

            await _imagingService.RunImagingSession(session);

        }
    }
}
