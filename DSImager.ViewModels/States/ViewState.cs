using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.ViewModels.States
{
    public abstract class ViewState : IViewState
    {
        private static Dictionary<string, ViewState> _states;

        public string Name { get; private set; }
        protected ViewState(string name)
        {
            Name = name;
            if(_states == null)
                _states = new Dictionary<string, ViewState>();
            _states.Add(name, this);
        }

        public override string ToString()
        {
            return Name;
        }

        public static ViewState FromString(string state)
        {
            if (_states.ContainsKey(state))
                return _states[state];
            
            return null;
        }

        public bool Equals(IViewState other)
        {
            if (other.GetType() == GetType())
            {
                ViewState state = (ViewState) other;
                if (state.Name == Name)
                    return true;
                return false;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((IViewState) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
