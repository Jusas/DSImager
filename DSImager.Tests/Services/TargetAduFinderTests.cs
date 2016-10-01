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
    public class TargetAduFinderTests
    {

        public int RequestNumber = 0;
        private int _camMaxAdu = 35000;

        private Mock<ICameraService> MockCameraService()
        {
            var service = new Mock<ICameraService>();
            var mockCam = new Mock<ICameraV2>();
            mockCam.SetupAllProperties();
            mockCam.Setup(x => x.CameraXSize).Returns(800);
            mockCam.Setup(x => x.CameraYSize).Returns(600);
            service.Setup(x => x.Camera).Returns(mockCam.Object);
            mockCam.SetupGet(x => x.MaxADU).Returns(_camMaxAdu);
            mockCam.SetupGet(x => x.ExposureMax).Returns(3600);

            int[][] imageArrs = new int[][]
            {
                new int[800 * 600],
                new int[800 * 600],
                new int[800 * 600]
            };

            for (int i = 0; i < 800; i++)
            {
                for (int j = 0; j < 600; j++)
                {
                    imageArrs[0][i * 600 + j] = 15000 + new Random().Next(0, 15);
                }
            }

            for (int i = 0; i < 800; i++)
            {
                for (int j = 0; j < 600; j++)
                {
                    imageArrs[1][i * 600 + j] = 10000 + new Random().Next(0, 15);
                }
            }

            for (int i = 0; i < 800; i++)
            {
                for (int j = 0; j < 600; j++)
                {
                    imageArrs[2][i * 600 + j] = 8750 + new Random().Next(0, 15);
                }
            }

            service.Setup(x => x.TakeExposure(It.IsAny<double>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Callback(() =>
                {
                    service.SetupGet(x => x.LastExposure)
                        .Returns(new Exposure(800, 600, imageArrs[RequestNumber], _camMaxAdu, false));
                    if(RequestNumber < imageArrs.Length - 1)
                        RequestNumber++;
                })
                .ReturnsAsync(true);

            return service;
        }

        [Test]
        public async Task TestFinder()
        {
            var moqCameraService = MockCameraService();
            var finder = new TargetAduFinder(moqCameraService.Object);
            var targetAdu = 8750;

            finder.OnFindExposureTaken += (time, adu) =>
            {
                Console.WriteLine("Find exposure taken, exposure time {0:F}s, ADUs {1}", time, adu);
            };

            var foundExposure = await finder.FindExposureValue(targetAdu, 3);

            moqCameraService.Verify(x => x.TakeExposure(It.IsAny<double>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Exactly(3));
        }

    }
}
