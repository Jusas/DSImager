using System;
using System.Collections.Generic;
using System.Windows.Input;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using DSImager.Core.System;

namespace DSImager.ViewModels
{
    /// <summary>
    /// ViewModel for the ConnectDialog.
    /// </summary>
    public class HistogramDialogViewModel : BaseViewModel<HistogramDialogViewModel>
    {
        //-------------------------------------------------------------------------------------------------------
        #region INTERNAL CLASSES AND DECLARATIONS
        //-------------------------------------------------------------------------------------------------------

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        private ICameraService _cameraService;
        private IImagingService _imagingService;
        
        private int _stretchMin = 0;
        public int StretchMin
        {
            get
            {
                return _stretchMin;
            }
            set
            {
                SetNotifyingProperty(() => StretchMin, ref _stretchMin, value);
            }
        }

        private int _stretchMax = 0;
        public int StretchMax
        {
            get
            {
                return _stretchMax;
            }
            set
            {
                SetNotifyingProperty(() => StretchMax, ref _stretchMax, value);
            }
        }
        
        private int _histogramMax = 65535;
        public int HistogramMax
        {
            get { return _histogramMax; }
            set
            {
                SetNotifyingProperty(() => HistogramMax, ref _histogramMax, value);
            }
        }

        private bool _useAutoStretch = true;
        public bool UseAutoStretch
        {
            get
            {
                return _useAutoStretch;
            }
            set
            {
                SetNotifyingProperty(() => UseAutoStretch, ref _useAutoStretch, value);
                _imagingService.ExposureVisualProcessingSettings.AutoStretch = value;
                // Apply auto stretch to the image.
                if (value && _cameraService.LastExposure != null)
                {
                    SetToAutoStretch();
                }

            }
        }

        private List<XY> _histogramPolyPoints;
        public List<XY> HistogramPolyPoints
        {
            get
            {
                return _histogramPolyPoints;
            }
            private set
            {
                SetNotifyingProperty(() => HistogramPolyPoints, ref _histogramPolyPoints, value);
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------


        public HistogramDialogViewModel(ILogService logService, ICameraService cameraService,
            IImagingService imagingService)
            : base(logService)
        {
            _cameraService = cameraService;
            _imagingService = imagingService;
        }

        public override void Initialize()
        {
            // Set to ImagingService, not to CameraService; ImagingService
            // manipulates CameraService's produced exposure settings and if we listened to this
            // event from CameraService we'd get the default values from exposure settings.
            _imagingService.OnImagingComplete += OnExposureCompleted;
            OwnerView.OnViewClosing += OnViewClosing;
            OwnerView.OnViewLoaded += OnViewLoaded;

        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PRIVATE METHODS
        //-------------------------------------------------------------------------------------------------------
        
        private void OnViewLoaded(object sender, EventArgs eventArgs)
        {
            StretchMax = _cameraService.ConnectedCamera.MaxADU;
            StretchMin = 0;

            _useAutoStretch = _imagingService.ExposureVisualProcessingSettings.AutoStretch;
            if (_cameraService.LastExposure != null)
            {
                InitHistogram(_cameraService.LastExposure);
            }
        }

        private void SetToAutoStretch()
        {
            var exposure = _cameraService.LastExposure;
            exposure.SetStretch();
            StretchMax = exposure.StretchMax;
            StretchMin = exposure.StretchMin;
        }

        private void InitHistogram(Exposure exposure)
        {
            StretchMax = exposure.StretchMax;
            StretchMin = exposure.StretchMin;

            HistogramMax = exposure.MaxDepth;
            List<XY> points = new List<XY>();
            for (int x = 0; x <= exposure.MaxDepth; x++)
            {
                double y = 0;
                if (exposure.Histogram.ContainsKey(x))
                    y = exposure.Histogram[x];
                points.Add(new XY { X = x, Y = y});
            }
            HistogramPolyPoints = points;
        }

        private void OnViewClosing(object sender, EventArgs eventArgs)
        {
            _imagingService.OnImagingComplete -= OnExposureCompleted;
        }


        private void OnExposureCompleted(bool successful, Exposure exposure)
        {
            if (exposure != null)
                InitHistogram(exposure);
        }

        private void DoImageStretch()
        {
            _cameraService.LastExposure.SetStretch(StretchMin, StretchMax);
            _imagingService.ExposureVisualProcessingSettings.StretchMin = StretchMin;
            _imagingService.ExposureVisualProcessingSettings.StretchMax = StretchMax;
        }

        
        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region COMMANDS
        //-------------------------------------------------------------------------------------------------------
        public ICommand DoStretchCommand { get { return new CommandHandler(DoImageStretch); } }
        #endregion

    }

    public class HistogramDialogViewModelDT : HistogramDialogViewModel
    {
        public HistogramDialogViewModelDT() : base(null, null, null)
        {
            
        }
    }
}
