using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DSImager.Core.Interfaces;
using DSImager.Core.Models;

namespace DSImager.Core.Services
{

    public class LogService : ILogService
    {
        private class HandlerCategoryPair
        {
            public LogMessageHandler Handler;
            public LogEventCategory Categories;
        }

        //-------------------------------------------------------------------------------------------------------
        #region FIELDS AND PROPERTIES
        //-------------------------------------------------------------------------------------------------------

        private const string _filename = "applog.txt";
        private FileStream _stream;
        private StreamWriter _streamWriter;

        private readonly object _mutex = new object();
        private readonly object _globalLogSource = new object();
        public object GlobalLogSource { get { return _globalLogSource; } }

        private Dictionary<object, List<HandlerCategoryPair>> _handlers = new Dictionary<object, List<HandlerCategoryPair>>();

        public string LogFile { get; private set; }

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------

        public LogService()
        {
            var appSettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DSImager");
            LogFile = Path.Combine(appSettingsFolder, _filename);
            OpenLogForWriting();
        }
        
        public void Trace(LogEventCategory category, string message)
        {
            lock (_mutex)
            {
                var now = DateTime.Now;
                var formatted = string.Format("[{0:D2}:{1:D2}:{2:D2}.{3:D3}] [{4}] {5}",
                    now.Hour, now.Minute, now.Second, now.Millisecond,
                    category, message);

                Console.WriteLine(formatted);
                _streamWriter.WriteLine(formatted);
                _streamWriter.Flush();
            }
        }


        private void OpenLogForWriting()
        {
            var appSettingsFolder = Path.GetDirectoryName(LogFile);

            if (!Directory.Exists(appSettingsFolder))
            {
                Directory.CreateDirectory(appSettingsFolder);
            }

            var now = DateTime.Now;
            if (!File.Exists(LogFile))
                _stream = File.Create(LogFile);
            else
                _stream = File.Open(LogFile, FileMode.Truncate, FileAccess.Write, FileShare.Read);
            
            _streamWriter = new StreamWriter(_stream);
            _streamWriter.WriteLine("====================================================================");
            _streamWriter.WriteLine("LOGGING SESSION STARTED");
            _streamWriter.WriteLine("DATE: " + now.ToString("s"));
            _streamWriter.WriteLine("====================================================================");
            _streamWriter.Flush();

        }

        // make sure this gets called
        public void Dispose()
        {
            if (_streamWriter != null)
            {
                _streamWriter.Flush();
                _streamWriter.Dispose();
            }
        }

        #endregion


        public void LogMessage(LogMessage logMessage)
        {
            lock (_mutex)
            {
                Trace(logMessage.Category, logMessage.Message);

                if (_handlers.ContainsKey(logMessage.EventSource) || _handlers.ContainsKey(GlobalLogSource))
                {
                    var handlerList =
                        _handlers.Where(h => h.Key == logMessage.EventSource || h.Key == GlobalLogSource)
                            .SelectMany(h => h.Value);
                    //var handlerList = _handlers[logMessage.EventSource];
                    foreach (var h in handlerList)
                    {
                        if ((h.Categories & logMessage.Category) > 0)
                        {
                            h.Handler(logMessage);
                        }
                    }
                }
            }
        }

        public void Subscribe(object logSource, LogEventCategory categories, LogMessageHandler handler)
        {
            lock (_mutex)
            {
                if (handler == null)
                    throw new ArgumentNullException("handler");
                if (logSource == null)
                    throw new ArgumentNullException("logSource");

                if (!_handlers.ContainsKey(logSource))
                    _handlers.Add(logSource, new List<HandlerCategoryPair>());

                var handlerCategoryPair = _handlers[logSource].Where(i => i.Handler == handler).FirstOrDefault();
                if (handlerCategoryPair != null)
                    handlerCategoryPair.Categories |= categories;
                else
                {
                    handlerCategoryPair = new HandlerCategoryPair() {Categories = categories, Handler = handler};
                    _handlers[logSource].Add(handlerCategoryPair);
                }
            }
        }

        public void UnSubscribe(object logSource, LogEventCategory categories, LogMessageHandler handler)
        {
            lock (_mutex)
            {
                if (handler == null)
                    throw new ArgumentNullException("handler");
                if (logSource == null)
                    throw new ArgumentNullException("logSource");

                if (!_handlers.ContainsKey(logSource))
                    return;

                var handlerCategoryPair = _handlers[logSource].Where(i => i.Handler == handler).FirstOrDefault();
                if (handlerCategoryPair == null)
                    return;

                var newCategories = handlerCategoryPair.Categories ^ categories;
                if (newCategories == 0)
                {
                    _handlers[logSource].Remove(handlerCategoryPair);
                    if (_handlers[logSource].Count == 0)
                        _handlers.Remove(logSource);
                }
                else
                {
                    handlerCategoryPair.Categories = newCategories;
                }
            }
        }
    }
}
