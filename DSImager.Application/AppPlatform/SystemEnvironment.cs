using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;

namespace DSImager.Application.AppPlatform
{
    public class SystemEnvironment : ISystemEnvironment
    {
        public string TemporaryFilesDirectory => Path.GetTempPath();

        public string UserHomeDirectory => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public string UserPicturesDirectory => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    }
}
