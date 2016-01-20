using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DSImager.Core.Interfaces;

namespace DSImager.ViewModels
{
    public class SessionDialogViewModel : BaseViewModel<SessionDialogViewModel>
    {

        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        private ICameraService _cameraService;
        private IImagingService _imagingService;

        private List<double> _presetExposureValues;
        public List<double> PresetExposureValues
        {
            get { return _presetExposureValues; }
            set
            {
                SetNotifyingProperty(() => PresetExposureValues, ref _presetExposureValues, value);
            }
        }

        private List<ImageFormat> _fileTypeOptions;
        public List<ImageFormat> FileTypeOptions
        {
            get { return _fileTypeOptions; }
            set
            {
                SetNotifyingProperty(() => FileTypeOptions, ref _fileTypeOptions, value);
            }
        }

        private List<KeyValuePair<int, string>> _binningModeOptions;
        /// <summary>
        /// Different binning modes available for the connected camera.
        /// </summary>
        public List<KeyValuePair<int, string>> BinningModeOptions
        {
            get { return _binningModeOptions; }
            set
            {
                SetNotifyingProperty(() => BinningModeOptions, ref _binningModeOptions, value);
            }
        }

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------

        public SessionDialogViewModel(ILogService logService, ICameraService cameraService,
            IImagingService imagingService)
            : base(logService)
        {
            _cameraService = cameraService;
            _imagingService = imagingService;
        }

        public override void Initialize()
        {
            OwnerView.OnViewLoaded += OnViewLoaded;            
        }

        private void OnViewLoaded(object sender, EventArgs eventArgs)
        {
            PopulateExposures();
            ConstructBinningOptions();
            ConstructFileTypeOptions();
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PRIVATE METHODS
        //-------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Populate the list of preset exposure values for the view to provide to the user.
        /// </summary>
        private void PopulateExposures()
        {
            const int scaleMax = 300;

            var minExposure = _cameraService.ConnectedCamera.ExposureMin;
            var maxExposure = _cameraService.ConnectedCamera.ExposureMax;

            if (Double.IsInfinity(maxExposure) || Double.IsNaN(maxExposure))
                maxExposure = scaleMax;

            var exposureValues = new List<double>();

            var currentVal = minExposure;
            while (currentVal < 1)
            {
                exposureValues.Add(currentVal);
                currentVal *= 10;
            }

            exposureValues.AddRange(
                (new double[] { 1, 2, 3, 5, 10, 15, 20, 30, 40, 50, 60, 90, 120, 180, 300, 600, 900 }).Where(d => d <= maxExposure));
            PresetExposureValues = exposureValues;

        }

        /// <summary>
        /// 
        /// </summary>
        private void ConstructFileTypeOptions()
        {
            FileTypeOptions = new List<ImageFormat>(_imagingService.SupportedImageFormats);
        }

        /// <summary>
        /// 
        /// </summary>
        private void ConstructBinningOptions()
        {
            var cam = _cameraService.ConnectedCamera;
            // For now assume we have equal X and Y binning. Otherwise assume no support.
            var maxBinning = cam.MaxBinX == cam.MaxBinY ? cam.MaxBinX : 1;

            List<KeyValuePair<int, string>> opts = new List<KeyValuePair<int, string>>();
            for (int i = 0; i < maxBinning; i++)
            {
                opts.Add(new KeyValuePair<int, string>(i + 1, string.Format("{0}x{0}", i + 1)));
            }

            BinningModeOptions = opts;
        }

        /// <summary>
        /// Makes sure the exposure is a valid value for the camera.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private double GetValidatedExposure(double val)
        {
            var minExposure = _cameraService.ConnectedCamera.ExposureMin;
            var maxExposure = _cameraService.ConnectedCamera.ExposureMax;

            if (val < minExposure)
                val = minExposure;
            if(!double.IsInfinity(maxExposure) && !double.IsNaN(maxExposure) && val > maxExposure)
                val = maxExposure;

            return val;
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateNewSessionEntry()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        private void CopySelectedSessionEntry()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        private void DeleteSelectedSessionEntry()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateNewSequenceEntry()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        private void CopySelectedSequenceEntry()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        private void DeleteSelectedSequenceEntry()
        {

        }

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region COMMANDS
        //-------------------------------------------------------------------------------------------------------

        public ICommand CreateNewSessionEntryCommand { get { return new CommandHandler(CreateNewSessionEntry); } }
        public ICommand CopySelectedSessionEntryCommand { get { return new CommandHandler(CopySelectedSessionEntry); } }
        public ICommand DeleteSelectedSessionEntryCommand { get { return new CommandHandler(DeleteSelectedSessionEntry); } }
        public ICommand CreateNewSequenceEntryCommand { get { return new CommandHandler(CreateNewSequenceEntry); } }
        public ICommand CopySelectedSequenceEntryCommand { get { return new CommandHandler(CopySelectedSequenceEntry); } }
        public ICommand DeleteSelectedSequenceEntryCommand { get { return new CommandHandler(DeleteSelectedSequenceEntry); } }

        #endregion

    }

    public class SessionDialogViewModelDT : SessionDialogViewModel
    {
        public SessionDialogViewModelDT() : base(null, null, null)
        {
        }
    }
}
