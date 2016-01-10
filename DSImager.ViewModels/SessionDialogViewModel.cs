using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;

namespace DSImager.ViewModels
{
    public class SessionDialogViewModel : BaseViewModel<SessionDialogViewModel>
    {
        public SessionDialogViewModel(ILogService logService)
            : base(logService)
        {
        }

        public override void Initialize()
        {
        }
    }

    public class SessionDialogViewModelDT : SessionDialogViewModel
    {
        public SessionDialogViewModelDT() : base(null)
        {
        }
    }
}
