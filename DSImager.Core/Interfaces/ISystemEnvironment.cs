using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{
    public interface ISystemEnvironment
    {
        string TemporaryFilesDirectory { get; }
        string UserHomeDirectory { get; }
        string UserPicturesDirectory { get; }
    }
}
