using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSImager.Core.Interfaces;

namespace DSImager.Core.Models
{
    public class LogMessage
    {
        /// <summary>
        /// The message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The message category
        /// </summary>
        public LogEventCategory Category { get; set; }

        /// <summary>
        /// The logger (source object)
        /// </summary>
        public object EventSource { get; set; }
        
        public LogMessage(object source, LogEventCategory category, string message)
        {
            EventSource = source;
            Category = category;
            Message = message;
        }
    }
}
