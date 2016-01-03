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
        Previewing
    }
    
    /*public class MainViewState : ViewState
    {        
        public static readonly MainViewState Idle = new MainViewState("Idle");
        public static readonly MainViewState Previewing = new MainViewState("Previewing");

        private MainViewState(string name) : base(name)
        {
        }
    }*/
}
