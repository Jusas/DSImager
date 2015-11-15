﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSImager.Core.Interfaces
{
    [Flags]
    public enum LogEventCategory
    {
        Verbose = 1 << 0,
        Informational = 1 << 1,
        Warning = 1 << 2,
        Error = 1 << 3
    }

    public delegate void LogMessageHandler(object logSource, LogEventCategory category, string message);

    public interface ILogService : IDisposable
    {
        /// <summary>
        /// A simple trace to the log file and to console.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="message"></param>
        void Trace(LogEventCategory category, string message);

        /// <summary>
        /// Logs an event.
        /// Performs standard trace and additionally notifies all listeners 
        /// subscribed to the eventSource.
        /// </summary>
        /// <param name="logSource">The logger (source object)</param>
        /// <param name="category">The category</param>
        /// <param name="message">The message</param>
        void LogMessage(object logSource, LogEventCategory category, string message);

        /// <summary>
        /// Subscribes to listen to log messages from a given source. All messages
        /// from the source will be relayed to the listener.
        /// </summary>
        /// <param name="logSource">The source to listen to</param>
        /// <param name="categories">The categories of messages the listener wants to receive</param>
        /// <param name="handler">The message handler</param>
        void Subscribe(object logSource, LogEventCategory categories, LogMessageHandler handler);

        /// <summary>
        /// Unsubscribes a message handler from the given log source and from the given categories.
        /// It is possible to remove a single or multiple categories from the handler.
        /// </summary>
        /// <param name="logSource"></param>
        /// <param name="categories"></param>
        /// <param name="handler"></param>
        void UnSubscribe(object logSource, LogEventCategory categories, LogMessageHandler handler);

    }
}
