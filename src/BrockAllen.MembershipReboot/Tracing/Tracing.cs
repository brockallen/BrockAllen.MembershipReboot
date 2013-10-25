/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Diagnostics;

namespace BrockAllen.MembershipReboot
{
    static public class Tracing
    {
        [DebuggerStepThrough]
        public static void Start(string message)
        {
            TraceEvent(TraceEventType.Start, message, false);
        }
        [DebuggerStepThrough]
        public static void Start(string message, params object[] args)
        {
            Start(String.Format(message, args));
        }

        [DebuggerStepThrough]
        public static void Stop(string message)
        {
            TraceEvent(TraceEventType.Stop, message, false);
        }
        [DebuggerStepThrough]
        public static void Stop(string message, params object[] args)
        {
            Stop(String.Format(message, args));
        }

        [DebuggerStepThrough]
        public static void Verbose(string message)
        {
            TraceEvent(TraceEventType.Verbose, message, false);
        }
        [DebuggerStepThrough]
        public static void Verbose(string message, params object[] args)
        {
            Verbose(String.Format(message, args));
        }
        
        [DebuggerStepThrough]
        public static void Information(string message)
        {
            TraceEvent(TraceEventType.Information, message, false);
        }
        [DebuggerStepThrough]
        public static void Information(string message, params object[] args)
        {
            Information(String.Format(message, args));
        }

        [DebuggerStepThrough]
        public static void Warning(string message)
        {
            TraceEvent(TraceEventType.Warning, message, false);
        }
        [DebuggerStepThrough]
        public static void Warning(string message, params object[] args)
        {
            Warning(String.Format(message, args));
        }

        [DebuggerStepThrough]
        public static void Error(string message)
        {
            TraceEvent(TraceEventType.Error, message, false);
        }
        [DebuggerStepThrough]
        public static void Error(string message, params object[] args)
        {
            Error(String.Format(message, args));
        }

        [DebuggerStepThrough]
        public static void TraceEvent(TraceEventType type, string message, bool suppressTraceService)
        {
            TraceSource ts = new TraceSource("MembershipReboot");

            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
            {
                if (type != TraceEventType.Verbose)
                {
                    Trace.CorrelationManager.ActivityId = Guid.NewGuid();
                }
            }

            ts.TraceEvent(type, 0, message);
        }
    }
}
