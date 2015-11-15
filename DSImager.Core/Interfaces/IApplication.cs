using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{
    public interface IApplication
    {
        event EventHandler OnAppStartUp;
        event EventHandler OnAppExit;
        event EventHandler<bool> BeforeAppExit; // todo ei näin, jotenki muuten eli pitää pystyä palauttamaan arvo joka katsotaan callerin päässä

        void ExitApplication(int exitCode);

    }
}
