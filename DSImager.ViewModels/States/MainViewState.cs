using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.ViewModels.States
{
    public enum MainViewState
    {
        Idle,
        Imaging
    }
    
    /*public class MainViewState : ViewState
    {        
        public static readonly MainViewState Idle = new MainViewState("Idle");
        public static readonly MainViewState Imaging = new MainViewState("Imaging");

        private MainViewState(string name) : base(name)
        {
        }
    }*/
}
