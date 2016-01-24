using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using Newtonsoft.Json;

namespace DSImager.Core.Models
{
    /**
     * Does not hold data, merely the metadata of the sequence.
     * Is also saved to disk as a sequence definition, ie. is serializable
     * and deserializable.
     */
    [JsonObject]
    public class ImageSequence
    {
        public string Name { get; set; }
        public int NumExposures { get; set; }
        public double ExposureDuration { get; set; }
        public string Extension { get; set; }
        public ImageFormat Format { get; set; }
        public bool Enabled { get; set; }

        public int BinXY { get; set; }

        [JsonIgnore]
        public int CurrentExposureIndex { get; set; }

        public ImageSequence()
        {
            Name = "untitled sequence";
            Enabled = true;
            NumExposures = 1;
            ExposureDuration = 1;
            Extension = "xxx";
            CurrentExposureIndex = 0;
            BinXY = 1;
            Format = ImageFormat.Fits;
        }

        public ImageSequence Clone()
        {
            return new ImageSequence
            {
                Name = Name,
                NumExposures = NumExposures,
                BinXY = BinXY,
                CurrentExposureIndex = 0,
                Enabled = Enabled,
                ExposureDuration = ExposureDuration,
                Extension = Extension,
                Format = Format
            };
        }

    }
}
