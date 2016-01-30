using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;

namespace DSImager.ViewModels
{
    public class SessionDialogViewModel : BaseViewModel<SessionDialogViewModel>
    {

        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        private ICameraService _cameraService;
        private IImagingService _imagingService;
        private IStorageService _storageService;
        private IImageIoService _imageIoService;

        private readonly string SessionFile = "saved-sessions.json";

        private List<double> _presetExposureValues;
        public List<double> PresetExposureValues
        {
            get { return _presetExposureValues; }
            set
            {
                SetNotifyingProperty(() => PresetExposureValues, ref _presetExposureValues, value);
            }
        }

        private List<string> _fileTypeOptionIds;
        public List<string> FileTypeOptionIds
        {
            get { return _fileTypeOptionIds; }
            set
            {
                SetNotifyingProperty(() => FileTypeOptionIds, ref _fileTypeOptionIds, value);
            }
        }

        private Dictionary<string, string> _fileTypeOptionNames;
        public Dictionary<string, string> FileTypeOptionNames
        {
            get { return _fileTypeOptionNames; }
            set
            {
                SetNotifyingProperty(() => FileTypeOptionNames, ref _fileTypeOptionNames, value);
            }
        }
        
        private List<int> _binningModeOptions;
        /// <summary>
        /// Different binning modes available for the connected camera.
        /// </summary>
        public List<int> BinningModeOptions
        {
            get { return _binningModeOptions; }
            set
            {
                SetNotifyingProperty(() => BinningModeOptions, ref _binningModeOptions, value);
            }
        }

        private ObservableCollection<ImagingSession> _savedSessions;
        /// <summary>
        /// Saved sessions, read from the sessions JSON file.
        /// </summary>
        public ObservableCollection<ImagingSession> SavedSessions
        {
            get { return _savedSessions; }
            set
            {
                SetNotifyingProperty(() => SavedSessions, ref _savedSessions, value);
            }
        }

        private int _selectedSessionIndex = -1;
        /// <summary>
        /// The selected session index.
        /// </summary>
        public int SelectedSessionIndex
        {
            get { return _selectedSessionIndex; }
            set
            {
                SetNotifyingProperty(() => SelectedSessionIndex, ref _selectedSessionIndex, value);
                SetNotifyingProperty(() => SelectedSession);
                SelectedSequenceIndex = -1;
            }
        }

        private int _selectedSequenceIndex = -1;
        /// <summary>
        /// The selected sequence index.
        /// </summary>
        public int SelectedSequenceIndex
        {
            get { return _selectedSequenceIndex; }
            set
            {
                SetNotifyingProperty(() => SelectedSequenceIndex, ref _selectedSequenceIndex, value);
                SetNotifyingProperty(() => SelectedSequence);
            }
        }

        /// <summary>
        /// The selected ImagingSession instance, fetched using the selected index.
        /// </summary>
        public ImagingSession SelectedSession
        {
            get
            {
                return (_savedSessions != null && _selectedSessionIndex >= 0 && _selectedSessionIndex < _savedSessions.Count) 
                    ? _savedSessions[_selectedSessionIndex] : null;
            }
            set
            {
                int index = _savedSessions.IndexOf(value);
                SelectedSessionIndex = index;
            }
        }

        /// <summary>
        /// The selected ImageSequence instance, fetched using the selected index.
        /// </summary>
        public ImageSequence SelectedSequence
        {
            get
            {
                return (SelectedSession != null && _selectedSequenceIndex >= 0 && _selectedSequenceIndex < SelectedSession.ImageSequences.Count)
                    ? SelectedSession.ImageSequences[_selectedSequenceIndex] : null;
            }
            set
            {
                int index = SelectedSession.ImageSequences.IndexOf(value);
                SelectedSequenceIndex = index;
            }
        }

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------

        public SessionDialogViewModel(ILogService logService, ICameraService cameraService,
            IImagingService imagingService, IStorageService storageService, IImageIoService imageIoService)
            : base(logService)
        {
            _cameraService = cameraService;
            _imagingService = imagingService;
            _storageService = storageService;
            _imageIoService = imageIoService;
        }

        public override void Initialize()
        {
            OwnerView.OnViewLoaded += OnViewLoaded;            
            OwnerView.OnViewClosing += OnViewClosing;
        }

        private void OnViewClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            _storageService.Set(SessionFile, SavedSessions);
        }

        private void OnViewLoaded(object sender, EventArgs eventArgs)
        {
            PopulateExposures();
            ConstructBinningOptions();
            ConstructFileTypeOptions();
            LoadSessionsFromDisk();
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PRIVATE METHODS
        //-------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Load the ImagingSessions from disk.
        /// </summary>
        private void LoadSessionsFromDisk()
        {
            try
            {
                SavedSessions = _storageService.Get<ObservableCollection<ImagingSession>>(SessionFile);
            }
            catch (Exception)
            {
                SavedSessions = new ObservableCollection<ImagingSession>();
                LogService.LogMessage(new LogMessage(this, LogEventCategory.Warning, 
                    "Imaging sessions could not be loaded from disk"));
            }
            
        }

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
            FileTypeOptionIds = new List<string>(_imageIoService.WritableFileFormats.Select(wff => wff.Id));
            FileTypeOptionNames = new Dictionary<string, string>();
            foreach(var ff in _imageIoService.WritableFileFormats)
                FileTypeOptionNames.Add(ff.Id, ff.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        private void ConstructBinningOptions()
        {
            var cam = _cameraService.ConnectedCamera;
            // For now assume we have equal X and Y binning. Otherwise assume no support.
            var maxBinning = cam.MaxBinX == cam.MaxBinY ? cam.MaxBinX : 1;

            List<int> opts = new List<int>();
            for (int i = 0; i < maxBinning; i++)
            {
                opts.Add(i + 1);
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
            SavedSessions.Add(new ImagingSession());
            SelectedSessionIndex = SavedSessions.Count - 1;
        }

        /// <summary>
        /// 
        /// </summary>
        private void CopySelectedSessionEntry()
        {
            SavedSessions.Add(SelectedSession.Clone());
            SelectedSessionIndex = SavedSessions.Count - 1;
        }

        /// <summary>
        /// 
        /// </summary>
        private void DeleteSelectedSessionEntry()
        {
            SavedSessions.Remove(SelectedSession);
            SelectedSessionIndex = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateNewSequenceEntry()
        {
            SelectedSession.ImageSequences.Add(new ImageSequence() { FileFormat = _imageIoService.WritableFileFormats.FirstOrDefault().Id });
            SelectedSequenceIndex = SelectedSession.ImageSequences.Count - 1;
        }

        /// <summary>
        /// 
        /// </summary>
        private void CopySelectedSequenceEntry()
        {
            SelectedSession.ImageSequences.Add(SelectedSequence.Clone());
            SelectedSequenceIndex = SelectedSession.ImageSequences.Count - 1;
        }

        /// <summary>
        /// 
        /// </summary>
        private void DeleteSelectedSequenceEntry()
        {
            SelectedSession.ImageSequences.Remove(SelectedSequence);
            SelectedSequenceIndex = -1;
        }

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        private void CloseDialog()
        {
            OwnerView.Close();
        }

        /// <summary>
        /// Starts the selected imaging session.
        /// </summary>
        private void StartImaging()
        {
            SelectedSession.SaveOutput = true;
            CloseDialog();
        }

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region COMMANDS
        //-------------------------------------------------------------------------------------------------------


        public ICommand CloseCommand { get { return new CommandHandler(CloseDialog); } }
        public ICommand StartCommand { get { return new CommandHandler(StartImaging); } }

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
        public SessionDialogViewModelDT() : base(null, null, null, null, null)
        {
        }
    }
}
