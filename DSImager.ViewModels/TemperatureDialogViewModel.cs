using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;

namespace DSImager.ViewModels
{
    public class TemperatureDialogViewModel : BaseViewModel<TemperatureDialogViewModel>
    {
        public TemperatureDialogViewModel(ILogService logService) : base(logService)
        {
        }
    }

    public class TemperatureDialogViewModelDT : TemperatureDialogViewModel
    {
        public TemperatureDialogViewModelDT() : base(null)
        { }
    }
}
