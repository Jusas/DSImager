using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Models;

namespace DSImager.Core.Interfaces
{
    /// <summary>
    /// Service for providing access to image reads and writes.
    /// For each file format we wish to support we must provide an image reader/writer implementation.
    /// </summary>
    public interface IImageIoService
    {
        IList<ImageFileFormat> ReadableFileFormats { get; }
        IList<ImageFileFormat> WritableFileFormats { get; }

        IImageWriter GetImageWriter(ImageFileFormat fileFormat);
        IImageReader GetImageReader(ImageFileFormat fileFormat);
        void RegisterImageReader(ImageFileFormat fileFormat, IImageReader readerImplementation);
        void RegisterImageWriter(ImageFileFormat fileFormat, IImageWriter writerImplementation);
    }
}
