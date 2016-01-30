using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Models
{
    /// <summary>
    /// A class representing a rough image file format.
    /// For example 16-bit Tiff or FITS.
    /// For the use of user interfaces and for registering image file formats into ImageIoService.
    /// </summary>
    public class ImageFileFormat
    {
        public string Id { get; private set; }
        public string Extension { get; private set; }
        public string Name { get; private set; }
        public string DialogString { get; private set; }

        public ImageFileFormat(string id, string name, string extension, string dialogString)
        {
            Id = id;
            Name = name;
            Extension = extension;
            DialogString = dialogString ?? "*." + extension;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
