using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using nom.tam.fits;
using nom.tam.util;

namespace DSImager.Core.System
{
    /// <summary>
    /// An image writer that writes FITS files.
    /// </summary>
    public class FitsWriter : IImageWriter
    {

        public ImageFileFormat Format { get; private set; }

        public FitsWriter()
        {
            Format = new ImageFileFormat("fits", "FITS", "fits", "FITS | *.fits");
        }

        public void Save(Exposure exposure, string filename)
        {
            Save(exposure, filename, null);
        }

        public void Save(Exposure exposure, string filename, Dictionary<string, object> metadata)
        {
            Fits fits = new Fits();
            int[] dimensions = new[] { exposure.Height, exposure.Width };

            
            // choose the type from calculating the bitness from the exposure maxdepth
            long a = 0, bits = -1;
            while (a < exposure.MaxDepth)
                a = (int)Math.Pow(2.0, ++bits);

            Array pixels;
            if (bits <= 16)
            {
                pixels = ConvertToShort(exposure.Pixels);
            }
            else
            {
                pixels = exposure.Pixels;
            }

            var data = ArrayFuncs.Curl(pixels, dimensions);
            var hdu = FitsFactory.HDUFactory(data);
            
            // Write the headers.

            BuildHeader(hdu.Header, exposure, metadata, a);

            fits.AddHDU(hdu);

            if (!filename.EndsWith(".fits"))
                filename += ".fits";
            
            BufferedFile bf = new BufferedFile(filename, FileAccess.ReadWrite, FileShare.None);
            fits.Write(bf);
            bf.Close();
        }

        private void BuildHeader(Header header, Exposure exposure, Dictionary<string, object> metadata, long maxValue)
        {
            // Set BZERO to half of bitness (32768 for 16-bit)
            header.AddValue("BZERO", 0.5 * maxValue, "");
            header.AddValue("BSCALE", (double)1, "");
            header.AddValue("DATAMIN", 0.0, "");
            header.AddValue("DATAMAX", (double)exposure.MaxDepth, "");
            header.AddValue("CBLACK", (double)exposure.PixelMinValue, "");
            header.AddValue("CWHITE", (double)exposure.PixelMaxValue, "");
            header.AddValue("SWCREATE", "DSImager", "");

            if (metadata != null)
            {
                foreach (var entry in metadata)
                {
                    if (entry.Value is int)
                        header.AddValue(entry.Key, (int)entry.Value, "");
                    if (entry.Value is bool)
                        header.AddValue(entry.Key, (bool)entry.Value, "");
                    if (entry.Value is double)
                        header.AddValue(entry.Key, (double)entry.Value, "");
                    if (entry.Value is string)
                        header.AddValue(entry.Key, (string)entry.Value, "");
                    if (entry.Value is long)
                        header.AddValue(entry.Key, (long)entry.Value, "");
                }    
            }
            
        }

        private short[] ConvertToShort(int[] ints)
        {
            short[] shorts = new short[ints.Length];
            for (var i = 0; i < ints.Length; i++)
            {
                // We live in a signed world. The input integer values start from 0 but in the output
                // 0 is actually half way. Therefore we need to add the MinValue to the input integer
                // to output the data properly for CSharpFits to save.
                shorts[i] = (short)(short.MinValue + ints[i]);
            }
            return shorts;
        }

    }
}
