﻿using System;
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
using Newtonsoft.Json.Linq;

namespace DSImager.ViewModels
{
    public class DarkFrameDialogViewModel : BaseViewModel<DarkFrameDialogViewModel>
    {
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
            ConstructRecentExposures();
            ConstructFileTypeOptions();
            ConstructBinningOptions();
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
            // TODO actions
            ImagingSession session = new ImagingSession()
            {
                SaveOutput = true,
                OutputDirectory = Settings.SavePath,
                Name = ImagingSession.Calibration
            };

            ImageSequence sequence = new ImageSequence()
            {
                Name = ImagingSession.Calibration,
                BinXY = Settings.BinningModeXY,
                Extension = "Dark",
                ExposureDuration = Settings.ExposureTime, // Zero should be acceptable for bias frames (== minimum exposure value) in ASCOM standard
                FileFormat = Settings.FileFormat,
                NumExposures = Settings.FrameCount,
            };
            session.ImageSequences.Add(sequence);

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

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region COMMANDS
        //-------------------------------------------------------------------------------------------------------

        public ICommand StartCaptureCommand { get { return new CommandHandler(StartCapture); } }
        public ICommand CancelCaptureCommand { get { return new CommandHandler(CancelCapture); } }
        public ICommand SelectOutputDirectoryCommand { get { return new CommandHandler(SelectOutputDirectory); } }

        #endregion
    }


    // ReSharper disable once InconsistentNaming
    public class DarkFrameDialogViewModelDT : DarkFrameDialogViewModel
    {
        public DarkFrameDialogViewModelDT() : base(null, null, null, null, null, null, null, null)
        {

        }
    }
}
