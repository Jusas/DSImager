using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using NUnit;
using NUnit.Framework;
using SimpleInjector;

namespace DSImager.Tests
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
            _container = Bootstrapper.Container;
            _storageService = _container.GetInstance<IStorageService>();
        }

        [Test]
        public void SaveAndRead()
        {
            string fn = "testJsonFile.json";
            var data = new TextData()
            {
                Items = new List<string> {"Hello", "World"},
                Name = "Test"
            };
            _storageService.Set(fn, data);
            var data2 = _storageService.Get<TextData>(fn);

            Assert.AreEqual(data2.Name, data.Name);
        }

    }
}
