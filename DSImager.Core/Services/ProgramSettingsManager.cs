using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using Newtonsoft.Json;

namespace DSImager.Core.Services
{
    public class ProgramSettingsManager : IProgramSettingsManager
    {
        private ISystemEnvironment _systemEnvironment;
        private ILogService _logService;
        private IStorageService _storageService;
        private readonly string _defaultSettingsFile = "program-settings.json";

        public ProgramSettingsManager(ISystemEnvironment systemEnvironment, ILogService logService,
            IStorageService storageService)
        {
            _systemEnvironment = systemEnvironment;
            _storageService = storageService;
            _logService = logService;
        }

        public void LoadSettings(string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
                filename = _defaultSettingsFile;

            try
            {
                Settings = _storageService.Get<ProgramSettings>(filename);
            }
            catch (IOException e)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Warning, "No settings file found, creating a new one"));
                Settings = new ProgramSettings();
            }
        }

        public void SaveSettings(string filename = null)
        {
            if (Settings == null)
            {
                try
                {
                    LoadSettings(filename);
                }
                catch (Exception e)
                {
                    _logService.LogMessage(new LogMessage(this, LogEventCategory.Error,
                        "Was unable to save program settings, the settings object is null"));
                    return;
                }
            }

            if (string.IsNullOrEmpty(filename))
                filename = _defaultSettingsFile;

            _storageService.Set(filename, Settings);

        }

        public ProgramSettings Settings { private set; get; }
    }
}
