using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;

namespace DSImager.Core.Services
{
    public class ImageIoService : IImageIoService
    {
        public IList<ImageFileFormat> ReadableFileFormats { get; private set; }
        public IList<ImageFileFormat> WritableFileFormats { get; private set; }

        private Dictionary<ImageFileFormat, IImageWriter> _writers = new Dictionary<ImageFileFormat, IImageWriter>();
        private Dictionary<ImageFileFormat, IImageReader> _readers = new Dictionary<ImageFileFormat, IImageReader>(); 

        public ImageIoService()
        {
            ReadableFileFormats = new List<ImageFileFormat>();
            WritableFileFormats = new List<ImageFileFormat>();
        }

        public IImageWriter GetImageWriter(ImageFileFormat fileFormat)
        {
            if (_writers.ContainsKey(fileFormat))
                return _writers[fileFormat];
            return null;
        }

        public IImageReader GetImageReader(ImageFileFormat fileFormat)
        {
            if (_readers.ContainsKey(fileFormat))
                return _readers[fileFormat];
            return null;
        }

        public void RegisterImageReader(ImageFileFormat fileFormat, IImageReader readerImplementation)
        {
            if(_readers.ContainsKey(fileFormat))
                throw new ArgumentException("Reader for the image format already registered!", "fileFormat");

            _readers.Add(fileFormat, readerImplementation);
            ReadableFileFormats.Add(fileFormat);
        }

        public void RegisterImageWriter(ImageFileFormat fileFormat, IImageWriter writerImplementation)
        {
            if (_writers.ContainsKey(fileFormat))
                throw new ArgumentException("Writer for the image format already registered!", "fileFormat");

            _writers.Add(fileFormat, writerImplementation);
            WritableFileFormats.Add(fileFormat);
        }
    }
}
