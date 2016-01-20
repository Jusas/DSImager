using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.System;
using Newtonsoft.Json;

namespace DSImager.Core.Models
{
    /// <summary>
    /// Imaging session is as a concept as it says, a session in which images
    /// are taken. It contains one or more ImageSequences. It contains settings
    /// regarding what to do when taking each ImageSequence (changing filter (not implemented yet),
    /// pausing, etc). Serializable class. TODO: filter may be actually wise to set in the ImageSequence.
    /// </summary>
    public class ImagingSession
    {
        public string Name { get; set; }
        public Rect AreaRect { get; set; }
        public List<ImageSequence> ImageSequences { get; set; }
        public bool PauseAfterEachSequence { get; set; }
        public int RepeatTimes { get; set; }
        public bool PauseAfterEachRepeat { get; set; }

        public ImagingSession()
        {
            ImageSequences = new List<ImageSequence>();
        }

        public ImagingSession(IEnumerable<ImageSequence> sequences)
        {
            ImageSequences = new List<ImageSequence>(sequences);
        }

        public string GenerateFilename(ImageSequence sequence)
        {
            // "Pleiades-16-01-01-192003_lum"
            return string.Format("{0}-{1}_{2}", Name, DateTime.Now.ToString("yy-MM-dd-HHmmss"),
                sequence.Extension);
        }
    }
}
