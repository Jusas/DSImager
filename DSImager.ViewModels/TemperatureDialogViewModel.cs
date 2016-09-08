using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DSImager.Core.Interfaces;
using DSImager.Core.System;

namespace DSImager.ViewModels
{
    public class TemperatureDialogViewModel : BaseViewModel<TemperatureDialogViewModel>
    {


        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------
        
        private ICameraService _cameraService;

        private int _graphTopValue = 0;
        /// <summary>
        /// The top value of the temperature graph.
        /// </summary>
        public int GraphTopValue
        {
            get { return _graphTopValue; }
            set { SetNotifyingProperty(() => GraphTopValue, ref _graphTopValue, value); }
        }

        private int _graphBottomValue = 0;
        /// <summary>
        /// The bottom value of the temperature graph.
        /// </summary>
        public int GraphBottomValue
        {
            get {  return _graphBottomValue; }
            set { SetNotifyingProperty(() => GraphBottomValue, ref _graphBottomValue, value); }
        }

        private List<XY> _temperatureGraphPointsList;
        /// <summary>
        /// Temperature graph points list.
        /// </summary>
        public List<XY> TemperatureGraphPointsList
        {
            get
            {
                return _temperatureGraphPointsList;
            }
            set
            {
                SetNotifyingProperty(() => TemperatureGraphPointsList, ref _temperatureGraphPointsList, value);
            }
        }

        private double _currentTemperature = 0;
        /// <summary>
        /// The current temperature of the CCD sensor.
        /// </summary>
        public double CurrentTemperature
        {
            get
            {
                return _currentTemperature;
            }
            set
            {
                SetNotifyingProperty(() => CurrentTemperature, ref _currentTemperature, value);
            }
        }

        private double _ambientTemperature = 0;
        /// <summary>
        /// The current HeatSink aka Ambient temperature (the temperature of the incoming air to the cooler).
        /// </summary>
        public double AmbientTemperature
        {
            get
            {
                return _ambientTemperature;
            }
            set
            {
                SetNotifyingProperty(() => AmbientTemperature, ref _ambientTemperature, value);
            }
        }

        private double _desiredTemperature = 0;

        public double DesiredTemperature
        {
            get { return _desiredTemperature; }
            set { SetNotifyingProperty(() => DesiredTemperature, ref _desiredTemperature, value); }
        }

        private bool _isCoolerOn = false;
        /// <summary>
        /// Indicates whether the cooler is on/off.
        /// </summary>
        public bool IsCoolerOn
        {
            get
            {
                return _isCoolerOn;
            }
            set
            {
                SetNotifyingProperty(() => IsCoolerOn, ref _isCoolerOn, value);
            }
        }

        private bool _isWarmingUp = false;
        /// <summary>
        /// Is the cooler actually warming up.
        /// </summary>
        public bool IsWarmingUp
        {
            get { return _isWarmingUp; }
            set { SetNotifyingProperty(() => IsWarmingUp, ref _isWarmingUp, value); }
        }

        private bool _displayThermalShockWarning = false;
        /// <summary>
        /// Toggle thermal shock warning pane visible/hidden.
        /// </summary>
        public bool DisplayThermalShockWarning
        {
            get
            {
                return _displayThermalShockWarning;
            }
            set
            {
                SetNotifyingProperty(() => DisplayThermalShockWarning, ref _displayThermalShockWarning, value);
            }
        }

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------

        public TemperatureDialogViewModel(ILogService logService, ICameraService cameraService) : base(logService)
        {
            _cameraService = cameraService;
            TemperatureGraphPointsList = new List<XY>();
        }

        public override void Initialize()
        {
            OwnerView.OnViewLoaded +=
                (sender, args) =>
                {
                    _cameraService.CameraTemperatureUpdateNotifier.CollectionChanged += TemperatureHistoryUpdated;
                    _cameraService.OnWarmUpStarted += OnCCDWarmUpStarted;
                    _cameraService.OnWarmUpCanceled += OnCCDWarmUpCanceled;
                    _cameraService.OnWarmUpCompleted += OnCCDWarmUpCompleted;
                    UpdateTemperatureData();
                    InitializeState();
                };
                    
            OwnerView.OnViewClosing +=
                (sender, args) =>
                {
                    _cameraService.CameraTemperatureUpdateNotifier.CollectionChanged -= TemperatureHistoryUpdated;
                    _cameraService.OnWarmUpStarted -= OnCCDWarmUpStarted;
                    _cameraService.OnWarmUpCanceled -= OnCCDWarmUpCanceled;
                    _cameraService.OnWarmUpCompleted -= OnCCDWarmUpCompleted;
                };
        }

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region PRIVATE METHODS
        //-------------------------------------------------------------------------------------------------------
        

        private void TemperatureHistoryUpdated(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            UpdateTemperatureData();
        }

        private void InitializeState()
        {
            IsCoolerOn = _cameraService.IsCoolerOn;
            IsWarmingUp = _cameraService.IsWarmingUp;
            DesiredTemperature = _cameraService.DesiredCCDTemperature;
            AmbientTemperature = _cameraService.AmbientTemperature;
        }

        private void UpdateTemperatureData()
        {
            // Calculate the scale of the graph (min-max), update the indicated top and bottom scale values
            // and update the graph points. The X-axis value is between 0..1 and the Y-axis 
            // value is between GraphTopValue..GraphBottomValue.

            var tempHistory = _cameraService.CameraTemperatureHistory;
            AmbientTemperature = _cameraService.AmbientTemperature;
            DesiredTemperature = _cameraService.DesiredCCDTemperature;
            
            double min = Double.MaxValue;
            double max = Double.MinValue;

            foreach (var t in tempHistory)
            {
                if (t < min)
                    min = t;
                if (t > max)
                    max = t;
            }

            double diff = 10 - (max - min);
            int addToScale = diff > 0 ? (int)(0.5 * diff) : 0;

            //GraphTopValue = (int)max + addToScale;
            //GraphBottomValue = (int) min - addToScale;
            GraphTopValue = (int)max;
            GraphBottomValue = (int)min;

            var xStep = 1.0 / tempHistory.Length;
            var points = new List<XY>();
            for (int i = 0; i < tempHistory.Length; i++)
            {
                points.Add(new XY() { X = xStep * i, Y = tempHistory[i] });
            }
            TemperatureGraphPointsList = points;

            CurrentTemperature = tempHistory.LastOrDefault();

        }

        private void OnCCDWarmUpStarted()
        {
            IsWarmingUp = true;
        }


        private void OnCCDWarmUpCanceled()
        {
            IsWarmingUp = false;
        }

        private void OnCCDWarmUpCompleted()
        {
            IsWarmingUp = false;
        }

        private void SetCoolerOn()
        {
            if (!_cameraService.IsCoolerOn)
            {
                _cameraService.SetCoolerOn(true);
                IsCoolerOn = true;
                _cameraService.SetDesiredCCDTemperature(DesiredTemperature);
            }
        }

        private void SetCoolerOffSafe()
        {
            if (_cameraService.IsCoolerOn)
            {
                var tempDiff = Math.Abs(_cameraService.CurrentCCDTemperature - _cameraService.AmbientTemperature);
                if (tempDiff >= 20)
                {
                    DisplayThermalShockWarning = true;
                }
                else
                {
                    SetCoolerOff();
                }
            }
        }

        private void SetCoolerOff()
        {
            if (_cameraService.IsCoolerOn)
            {
                _cameraService.SetCoolerOn(false);
                IsCoolerOn = false;
            }

            if (DisplayThermalShockWarning)
                DisplayThermalShockWarning = false;
        }

        private void CancelCoolerOff()
        {
            if (DisplayThermalShockWarning)
                DisplayThermalShockWarning = false;
        }

        private void SetDesiredCCDTemperature()
        {
            if (_cameraService.Camera != null)
                _cameraService.SetDesiredCCDTemperature(DesiredTemperature);
        }

        private void WarmUp()
        {
            if(!_cameraService.IsWarmingUp)
                _cameraService.WarmUp();
        }

        private void CancelWarmUp()
        {
            if (_cameraService.IsWarmingUp)
                _cameraService.CancelWarmUp();
        }

        private void DecrementDesiredTemperature()
        {
            DesiredTemperature -= 5;
            SetDesiredCCDTemperature();
        }

        private void IncrementDesiredTemperature()
        {
            DesiredTemperature += 5;
            SetDesiredCCDTemperature();
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region COMMANDS
        //-------------------------------------------------------------------------------------------------------

        public ICommand SetCoolerOnCommand { get { return new CommandHandler(SetCoolerOn); } }
        public ICommand SetCoolerOffSafeCommand { get { return new CommandHandler(SetCoolerOffSafe); } }
        public ICommand CancelCoolerOffCommand { get { return new CommandHandler(CancelCoolerOff); } }
        public ICommand ConfirmCoolerOffCommand { get { return new CommandHandler(SetCoolerOff); } }
        public ICommand SetDesiredCCDTemperatureCommand { get { return new CommandHandler(SetDesiredCCDTemperature); } }
        public ICommand WarmUpCommand { get { return new CommandHandler(WarmUp); } }
        public ICommand CancelWarmUpCommand { get { return new CommandHandler(CancelWarmUp); } }
        public ICommand DecrementDesiredTemperatureCommand { get { return new CommandHandler(DecrementDesiredTemperature); } }
        public ICommand IncrementDesiredTemperatureCommand { get { return new CommandHandler(IncrementDesiredTemperature); } }

        #endregion


    }

    public class TemperatureDialogViewModelDT : TemperatureDialogViewModel
    {
        public TemperatureDialogViewModelDT() : base(null, null)
        { }
    }
}

