using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{
    public interface IStorageService
    {
        T Get<T>(string filename);
        void Set(string filename, object data);
    }
}
