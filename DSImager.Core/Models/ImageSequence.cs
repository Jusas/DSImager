﻿using System;
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
    public class ImageSequence
    {
        public string Name { get; set; }
        public int NumExposures { get; set; }
        public double ExposureDuration { get; set; }
        public string Extension { get; set; }
        public ImageFormat Format { get; set; }

        public int BinX { get; set; }
        public int BinY { get; set; }

        [JsonIgnore]
        public int CurrentExposure { get; set; }

        public ImageSequence()
        {
            Name = "untitled sequence";
            NumExposures = 1;
            ExposureDuration = 1;
            Extension = "xxx";
            CurrentExposure = 0;
            BinX = 1;
            BinY = 1;
            Format = ImageFormat.Fits;
        }

    }
}
