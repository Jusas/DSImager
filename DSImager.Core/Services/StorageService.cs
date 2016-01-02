using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;
using Newtonsoft.Json;

namespace DSImager.Core.Services
{
    public class StorageService : IStorageService
    {
        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        private ILogService _logService;
        private string _rootPath;

        #endregion

        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------

        public StorageService(ILogService logService)
        {
            _logService = logService;
            _rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DSImager");
        }

        public void SetStorageRoot(string path)
        {
            _rootPath = path;
        }

        public T Get<T>(string filename)
        {
            try
            {
                var data = File.ReadAllText(Path.Combine(_rootPath, filename));
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (IOException e)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error, 
                    "StorageService.Get IO Exception: " + e.Message));
                throw;
            }
            catch (JsonException e)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error,
                    "StorageService.Get JSON Deserialization exception: " + e.Message));
                throw;
            }
            catch (Exception e)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error,
                    "StorageService.Get Unexpected exception: " + e.Message));
                throw;
            }
        }

        public void Set(string filename, object data)
        {
            var fname = Path.Combine(_rootPath, filename);
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(fname)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fname));
                }
                var json = JsonConvert.SerializeObject(data);
                File.WriteAllText(fname, json);
            }
            catch (IOException e)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error,
                    "StorageService.Set IO Exception: " + e.Message));
                throw;
            }
            catch (JsonException e)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error,
                    "StorageService.Set JSON Deserialization exception: " + e.Message));
                throw;
            }
            catch (Exception e)
            {
                _logService.LogMessage(new LogMessage(this, LogEventCategory.Error,
                    "StorageService.Set Unexpected exception: " + e.Message));
                throw;
            }
        }

        #endregion
    }
}
