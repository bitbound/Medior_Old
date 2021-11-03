using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Medior.Core.PhotoSorter.Services;

namespace Medior.Tests
{
    [TestClass]
    public class MetadataReaderTests
    {
        private MetadataReader _metadataReader = new();


        [TestInitialize]
        public void Init()
        {
            _metadataReader = new MetadataReader();
        }

        [TestMethod]
        public void ParseExifDateTime_GivenInvalidData_Fails()
        {
            var result = _metadataReader.ParseExifDateTime(string.Empty);
            Assert.IsFalse(result.IsSuccess);

            result = _metadataReader.ParseExifDateTime(" ");
            Assert.IsFalse(result.IsSuccess);

            result = _metadataReader.ParseExifDateTime("2021:09");
            Assert.IsFalse(result.IsSuccess);

            result = _metadataReader.ParseExifDateTime("2021-09-24");
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void ParseExifDateTime_GivenValidData_Succeeds()
        {
            var result = _metadataReader.ParseExifDateTime("2021:09:24 13:27:34");
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(new DateTime(2021, 9, 24, 13, 27, 34), result.Value);

            result = _metadataReader.ParseExifDateTime("2021:09:24");
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(new DateTime(2021, 9, 24), result.Value);
        }

        [TestMethod]
        public void TryGetExifData_GivenInvalidPath_Fails()
        {
            var result = _metadataReader.TryGetExifData(string.Empty);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("File could not be found.", result.Error);

            result = _metadataReader.TryGetExifData(AppContext.BaseDirectory);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("File could not be found.", result.Error);

            result = _metadataReader.TryGetExifData(Path.Combine(AppContext.BaseDirectory, "Resources", "PicWithoutExif.jpg"));
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("DateTime is missing from metadata.", result.Error);
        }

        [TestMethod]
        public void TryGetExifData_GivenValidPath_Succeeds()
        {
            var result = _metadataReader.TryGetExifData(Path.Combine(AppContext.BaseDirectory, "Resources", "PicWithExif.jpg"));
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(result?.Value?.DateTaken, new DateTime(2015, 11, 14, 14, 41, 14));
            Assert.AreEqual(result?.Value?.CameraModel, "Lumia 640 LTE");
        }
    }
}
