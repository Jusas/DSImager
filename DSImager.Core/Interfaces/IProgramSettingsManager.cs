using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Models;

namespace DSImager.Core.Interfaces
{
    public interface IProgramSettingsManager
    {
        void LoadSettings(string filename = null);
        void SaveSettings(string filename = null);
        
        ProgramSettings Settings { get; }
    }
}
