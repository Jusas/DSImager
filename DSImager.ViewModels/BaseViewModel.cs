using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using DSImager.Core.Interfaces;
using DSImager.Core.Utils;

namespace DSImager.ViewModels
{
    /// <summary>
    /// A nice wrapper that enables a bit easier property change notifications
    /// for ViewModel (-) View bindings.
    /// </summary>
    public abstract class BaseViewModel<TViewModel> : IViewModel<TViewModel>, 
        INotifyPropertyChanged where TViewModel:BaseViewModel<TViewModel>
    {
        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        public event PropertyChangedEventHandler PropertyChanged;

        public TViewModel ViewModel
        {
            get { return this as TViewModel; }
        }

        private IView<TViewModel> _ownerView;
        public IView<TViewModel> OwnerView
        {
            set
            {
                _ownerView = value;
                if (value != null)
                {
                    OnOwnerViewSet(value);
                }
            }
            get
            {
                return _ownerView;
            }
        }
        protected ILogService LogService;

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region PROTECTED METHODS
        //-------------------------------------------------------------------------------------------------------

        public BaseViewModel(ILogService logService)
        {
            LogService = logService;
        }

        public virtual void Initialize()
        {
            
        }

        internal virtual void OnOwnerViewSet(IView<TViewModel> view)
        {
        }
        
        protected void SetNotifyingProperty<T>(Expression<Func<T>> expression, ref T field, T value)
        {
            if (field == null || !field.Equals(value))
            {
                T oldValue = field;
                field = value;
                OnPropertyChanged(this, new PropertyChangedExtendedEventArgs<T>(GetPropertyName(expression), oldValue, value));
            }
        }

        /// <summary>
        /// A notification method for a property that doesn't have a backing field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="value"></param>
        protected void SetNotifyingProperty<T>(Expression<Func<T>> expression, T value)
        {
            var oldValue = expression.Compile().Invoke();
            if (!oldValue.Equals(value))
            {
                OnPropertyChanged(this, new PropertyChangedExtendedEventArgs<T>(GetPropertyName(expression), oldValue, value));
            }
        }

        protected void SetNotifyingProperty<T>(Expression<Func<T>> expression)
        {
            OnPropertyChanged(this, new PropertyChangedEventArgs(GetPropertyName(expression)));
        }

        protected string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member.Name;
        }

        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(sender, e);
        }

        #endregion

    }
}
