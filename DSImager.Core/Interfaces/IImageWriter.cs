﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Models;

namespace DSImager.Core.Interfaces
{
    public interface IImageWriter
    {
        void Save(Exposure exposure, string filename);
        void Save(Exposure exposure, string filename, Dictionary<string, object> metadata);

    }
}
