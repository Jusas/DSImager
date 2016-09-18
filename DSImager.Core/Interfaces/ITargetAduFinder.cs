using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{
    public delegate void FindExposureTakenHandler(double usedExposureTime, int resultingAdu);

    public interface ITargetAduFinder
    {
        event FindExposureTakenHandler OnFindExposureTaken;
        Task<double> FindExposureValue(int targetAdu, double maxExposure);
        void CancelFindOperation();
    }
}
