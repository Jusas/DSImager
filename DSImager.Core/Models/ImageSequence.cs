using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public int NumExposures { get; set; }
        public double ExposureDuration { get; set; }
        public string Extension { get; set; }

        [JsonIgnore]
        public int CurrentExposure { get; set; }

        public ImageSequence()
        {
            NumExposures = 1;
            ExposureDuration = 1;
            Extension = "";
            CurrentExposure = 0;
        }

    }
}
