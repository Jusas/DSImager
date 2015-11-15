using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DSImager.Core.Interfaces;

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

        private Dictionary<object, List<HandlerCategoryPair>> _handlers = new Dictionary<object, List<HandlerCategoryPair>>();

        #endregion


        //-------------------------------------------------------------------------------------------------------
        #region PUBLIC METHODS
        //-------------------------------------------------------------------------------------------------------

        public LogService()
        {
            OpenLogForWriting();
        }
        
        public void Trace(LogEventCategory category, string message)
        {
            var now = DateTime.Now;
            var formatted = string.Format("[{0:D2}:{1:D2}:{2:D2}.{3:D3}] [{4}] {5}",
                now.Hour, now.Minute, now.Second, now.Millisecond,
                category, message);

            Console.WriteLine(formatted);
            _streamWriter.WriteLine(formatted);
            _streamWriter.Flush();
        }


        private void OpenLogForWriting()
        {
            var appSettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DSImager");
            var logFile = Path.Combine(appSettingsFolder, _filename);

            if (!Directory.Exists(appSettingsFolder))
            {
                Directory.CreateDirectory(appSettingsFolder);
            }

            var now = DateTime.Now;
            _stream = File.Open(logFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            
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


        public void LogMessage(object logSource, LogEventCategory category, string message)
        {
            Trace(category, message);

            if (_handlers.ContainsKey(logSource))
            {
                var handlerList = _handlers[logSource];
                foreach (var h in handlerList)
                {
                    if ((h.Categories & category) > 0)
                    {
                        h.Handler(logSource, category, message);
                    }
                }
            }
        }

        public void Subscribe(object logSource, LogEventCategory categories, LogMessageHandler handler)
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

        public void UnSubscribe(object logSource, LogEventCategory categories, LogMessageHandler handler)
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
