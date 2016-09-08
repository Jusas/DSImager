using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
    public class StorageServiceTests
    {

        private IStorageService _storageService;
        private Container _container;

        class TextData
        {
            public string Name { get; set; }
            public List<string> Items { get; set; }
        }

        [OneTimeSetUp]
        public void Init()
        {
            var moqLogService = new Mock<ILogService>();
            _storageService = new StorageService(moqLogService.Object);
        }

        [Test]
        public void SaveAndRead()
        {
            string fileName = Path.GetTempFileName();
            var data = new TextData()
            {
                Items = new List<string> {"Hello", "World"},
                Name = "Test"
            };
            _storageService.Set(fileName, data);
            var data2 = _storageService.Get<TextData>(fileName);

            Assert.AreEqual(data.Name, data2.Name);
            Assert.AreEqual(data.Items.Count, data2.Items.Count);
        }

    }
}
