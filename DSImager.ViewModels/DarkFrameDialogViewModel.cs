using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using DSImager.Core.Utils;
using Newtonsoft.Json.Linq;

namespace DSImager.ViewModels
{
    public class DarkFrameDialogViewModel : BaseViewModel<DarkFrameDialogViewModel>
    {
        //-------------------------------------------------------------------------------------------------------
        #region HELPER CLASSES
        //-------------------------------------------------------------------------------------------------------

        /// <summary>
        /// A helper class for selecting image sequences in the grid control.
        /// </summary>
        public class SelectableImageSequence
        {
            public string FormattedName { get; set; }
            public ImageSequence Sequence { get; set; }
            public bool IsSelected { get; set; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        private ICameraService _cameraService;
        private IImagingService _imagingService;
        private IStorageService _storageService;
        private IImageIoService _imageIoService;
        private IDialogProvider _dialogProvider;
        private ISystemEnvironment _systemEnvironment;
        private IProgramSettingsManager _programSettingsManager;

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


        private ProgramSettings.DarkFrameDialogSettings _settings;
        public ProgramSettings.DarkFrameDialogSettings Settings
        {
            get { return _settings; }
            set
            {
                SetNotifyingProperty(() => Settings, ref _settings, value);
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

        public Dictionary<double, string> RecentExposureSequenceTimesToNames { get; private set; }

        private List<double> _recentExposureTimes = new List<double>();
        public List<double> RecentExposureTimes
        {
            get
            {
                return _recentExposureTimes;
            }
            set
            {
                SetNotifyingProperty(() => RecentExposureTimes, ref _recentExposureTimes, value);
            }
        }


        private List<SelectableImageSequence> _sourceImageSequences;
        public List<SelectableImageSequence> SourceImageSequences
        {
            get
            {
                return _sourceImageSequences;
            }
            set
            {
                SetNotifyingProperty(() => SourceImageSequences, ref _sourceImageSequences, value);
            }
        }

        private bool _isSelectingSourceSession;
        public bool IsSelectingSourceSession
        {
            get
            {
                return _isSelectingSourceSession;
            }
            set
            {
                SetNotifyingProperty(() => IsSelectingSourceSession, ref _isSelectingSourceSession, value);
            }
        }

        private ImagingSession _selectedSourceSession;
        public ImagingSession SelectedSourceSession
        {
            get
            {
                return _selectedSourceSession;
            }
            set
            {
                SetNotifyingProperty(() => SelectedSourceSession, ref _selectedSourceSession, value);
                SourceImageSequences = SelectedSourceSession != null
                    ? SelectedSourceSession.ImageSequences.Select(s => new SelectableImageSequence
                    {
                        FormattedName = s.Name,
                        IsSelected = s.Enabled,
                        Sequence = s
                    }).ToList()
                    : new List<SelectableImageSequence>();
            }
        }

        private List<ImagingSession> _sourceSessions;
        public List<ImagingSession> SourceSessions
        {
            get
            {
                return _sourceSessions;
            }
            set
            {
                SetNotifyingProperty(() => SourceSessions, ref _sourceSessions, value);
                SetNotifyingProperty(() => HasAvailableSessions);
            }
        }

        public bool HasAvailableSessions => _sourceSessions != null && _sourceSessions.Count > 0;

        /*
        private double _selectedExposureTime;
        public double SelectedExposureTime
        {
            get
            {
                return _selectedExposureTime;
            }
            set
            {
                SetNotifyingProperty(() => SelectedExposureTime, ref _selectedExposureTime, value);
            }
        }*/

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------

        public DarkFrameDialogViewModel(ILogService logService, ICameraService cameraService,
            IImagingService imagingService, IStorageService storageService, IImageIoService imageIoService,
            IDialogProvider dialogProvider, ISystemEnvironment systemEnvironment,
            IProgramSettingsManager programSettingsManager) : base(logService)
        {
            _cameraService = cameraService;
            _imagingService = imagingService;
            _storageService = storageService;
            _imageIoService = imageIoService;
            _dialogProvider = dialogProvider;
            _systemEnvironment = systemEnvironment;
            _programSettingsManager = programSettingsManager;

            _settings = new ProgramSettings.DarkFrameDialogSettings();
        }

        public override void Initialize()
        {
            OwnerView.OnViewLoaded += OnViewLoaded;
            OwnerView.OnViewClosing += OnViewClosing;
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PRIVATE METHODS
        //-------------------------------------------------------------------------------------------------------

        private void OnViewClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            SaveSettings();
        }

        private void OnViewLoaded(object sender, EventArgs eventArgs)
        {
            LoadSettings();
            LoadSessionsFromDisk();
            ConstructRecentExposures();
            ConstructFileTypeOptions();
            ConstructBinningOptions();
        }

        private void LoadSessionsFromDisk()
        {
            try
            {
                SourceSessions = _storageService.Get<List<ImagingSession>>(SessionDialogViewModel.SessionFile); // TODO: move sessions file name declaration elsewhere? make a manager?
            }
            catch (Exception)
            {
                SourceSessions = new List<ImagingSession>();
            }
        }

        private void LoadSettings()
        {
            Settings = _programSettingsManager.Settings.DarkFrameDialog;
        }
        
        private void SaveSettings()
        {
            _programSettingsManager.SaveSettings();
        }

        private void ConstructRecentExposures()
        {
            var times = new List<double>();
            RecentExposureSequenceTimesToNames = new Dictionary<double, string>();

            var validSessions = _imagingService.SessionHistory.Where(
                s => s.Name != ImagingSession.Untitled && s.Name != ImagingSession.Calibration).ToArray().Reverse().ToArray();
            var imageSequences = validSessions.SelectMany(s => s.ImageSequences).ToArray();
            var durationGroups = imageSequences.GroupBy(s => s.ExposureDuration).ToArray();

            foreach (var item in durationGroups)
            {
                var seqNames = item.Select(s => s.Name);
                var text = string.Join(", ", seqNames);
                RecentExposureTimes.Add(item.Key);
                RecentExposureSequenceTimesToNames.Add(item.Key, $"{item.Key:F}s - {text}");
            }
            
            RecentExposureTimes = times;
        }

        private void ConstructBinningOptions()
        {
            var cam = _cameraService.Camera;
            // For now assume we have equal X and Y binning. Otherwise assume no support.
            var maxBinning = cam.MaxBinX == cam.MaxBinY ? cam.MaxBinX : 1;

            List<int> opts = new List<int>();
            for (int i = 0; i < maxBinning; i++)
            {
                opts.Add(i + 1);
            }

            BinningModeOptions = opts;
        }

        private void ConstructFileTypeOptions()
        {
            FileTypeOptionIds = new List<string>(_imageIoService.WritableFileFormats.Select(wff => wff.Id));
            FileTypeOptionNames = new Dictionary<string, string>();
            foreach (var ff in _imageIoService.WritableFileFormats)
                FileTypeOptionNames.Add(ff.Id, ff.Name);
        }

        private void StartCapture()
        {

            ImagingSession session = new ImagingSession()
            {
                SaveOutput = true,
                Name = ImagingSession.Calibration
            };

            if (SelectedSourceSession == null)
            {
                session.OutputDirectory = Settings.SavePath;
                ImageSequence sequence = new ImageSequence()
                {
                    Name = ImagingSession.Calibration,
                    BinXY = Settings.BinningModeXY,
                    Extension = "Dark",
                    ExposureDuration = Settings.ExposureTime,
                    FileFormat = Settings.FileFormat,
                    NumExposures = Settings.FrameCount
                };
                session.ImageSequences.Add(sequence);
            }
            else
            {
                session.OutputDirectory = Settings.SavePath;
                session.PauseAfterEachSequence = false;
                session.RepeatTimes = 1;
                foreach (var s in SourceImageSequences)
                {
                    if (s.IsSelected)
                    {
                        var sequence = s.Sequence.Clone();
                        sequence.Name = ImagingSession.Calibration;
                        sequence.Enabled = true;
                        sequence.Extension = SelectedSourceSession.Name.ToFilenameString() + "__" + s.Sequence.Name.ToFilenameString() + "__Dark";
                        sequence.NumExposures = Settings.FrameCount;
                        session.ImageSequences.Add(sequence);
                    }
                }
            }
            
            _imagingService.RunImagingSession(session);

            OwnerView.DialogResult = true;
            OwnerView.Close();
        }

        private void CancelCapture()
        {
            OwnerView.DialogResult = false;
            OwnerView.Close();
        }

        /// <summary>
        /// Select output directory from a directory dialog.
        /// </summary>
        private void SelectOutputDirectory()
        {
            string directory = string.IsNullOrEmpty(Settings.SavePath) ? _systemEnvironment.UserPicturesDirectory : Settings.SavePath;
            bool ok = _dialogProvider.ShowPickDirectoryDialog("Select output directory", directory,
                out directory);

            if (ok && !string.IsNullOrEmpty(directory))
            {
                Settings.SavePath = directory;
                SetNotifyingProperty(() => Settings);
            }
        }

        private void SelectSourceSession()
        {
            IsSelectingSourceSession = false;
        }

        private void StartSelectingSourceSession()
        {
            SelectedSourceSession = null;
            IsSelectingSourceSession = true;
        }

        private void ResetSourceSession()
        {
            SelectedSourceSession = null;
            IsSelectingSourceSession = false;
        }


        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region COMMANDS
        //-------------------------------------------------------------------------------------------------------

        public ICommand StartCaptureCommand { get { return new CommandHandler(StartCapture); } }
        public ICommand CancelCaptureCommand { get { return new CommandHandler(CancelCapture); } }
        public ICommand SelectOutputDirectoryCommand { get { return new CommandHandler(SelectOutputDirectory); } }
        public ICommand SelectSourceSessionCommand { get { return new CommandHandler(SelectSourceSession);} }
        public ICommand StartSelectingSourceSessionCommand { get { return new CommandHandler(StartSelectingSourceSession);} }
        public ICommand ResetSourceSessionCommand { get { return new CommandHandler(ResetSourceSession); } }

        #endregion
    }


    // ReSharper disable once InconsistentNaming
    public class DarkFrameDialogViewModelDT : DarkFrameDialogViewModel
    {
        public DarkFrameDialogViewModelDT() : base(null, null, null, null, null, null, null, null)
        {
            SourceImageSequences = new List<SelectableImageSequence>()
            {
                new SelectableImageSequence()
                {
                    FormattedName = "red (test session)",
                    IsSelected = false,
                    Sequence = new ImageSequence()
                    {
                        BinXY = 1,
                        ExposureDuration = 60
                    }
                }
            };
        }
    }
}
