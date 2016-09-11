using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSImager.Core.Models
{

    [JsonObject]
    public class ProgramSettings
    {
        public class BiasFrameDialogSettings
        {
            public string SavePath { get; set; }
            public int BinningModeXY { get; set; }
            public int FrameCount { get; set; }
            public string FileFormat { get; set; }
        }

        public BiasFrameDialogSettings BiasFrameDialog { get; set; }

        public ProgramSettings()
        {
            BiasFrameDialog = new BiasFrameDialogSettings();
        }

    }
}
