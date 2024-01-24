// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Meta.PerformanceSettings
{
    public class LogSection : MonoBehaviour
    {
        public enum EType
        {
            Callback,
            Log,
            Warning,
            Error,
        }

        private struct LogData
        {
            public DateTime Timestamp;
            public EType LogType;
            public string Message;
            public string Details;
        }

        public string TimestampFormat;
        public LogLine LogLinePrefab;
        public RectTransform LogLinesParent;
        public Color CallbackColor, LogColor, WarningColor, ErrorColor;
        private List<LogData> m_newLogs = new();
        private StringBuilder m_logSB = new();

        private void Awake()
        {
            for (var i = LogLinesParent.childCount - 1; i >= 0; --i)
                Destroy(LogLinesParent.GetChild(i).gameObject);

            Application.logMessageReceivedThreaded += LogMessageReceivedCallback;
            OVRManager.HMDMounted += () => { LogCallback("HMDMounted"); };
            OVRManager.HMDUnmounted += () => { LogCallback("HMDUnmounted"); };
            OVRManager.VrFocusAcquired += () => { LogCallback("VrFocusAcquired"); };
            OVRManager.VrFocusLost += () => { LogCallback("VrFocusLost"); };
            OVRManager.InputFocusAcquired += () => { LogCallback("InputFocusAcquired"); };
            OVRManager.InputFocusLost += () => { LogCallback("InputFocusLost"); };
            OVRManager.TrackingAcquired += () => { LogCallback("TrackingAcquired"); };
            OVRManager.TrackingLost += () => { LogCallback("TrackingLost"); };
        }

        private void LogMessageReceivedCallback(string logString, string stackTrace, LogType type)
        {
            var val = new LogData
            {
                Timestamp = DateTime.Now,
                LogType = UnityLogTypeToThisLogType(type),
                Message = logString,
                Details = stackTrace
            };
            m_newLogs.Add(val);
        }

        private void LogCallback(string logString)
        {
            var val = new LogData
            {
                Timestamp = DateTime.Now,
                LogType = EType.Callback,
                Message = logString
            };
            var st = new StackTrace(true);
            var sf = st.GetFrame(2);
            if (sf != null)
                val.Details = string.Format("{0} {1} Line: {2}", sf.GetMethod(), sf.GetFileName(), sf.GetFileLineNumber());
            m_newLogs.Add(val);
        }

        private EType UnityLogTypeToThisLogType(LogType logType)
        {
            return logType switch
            {
                LogType.Exception or LogType.Assert or LogType.Error => EType.Error,
                LogType.Warning => EType.Warning,
                LogType.Log => EType.Log,
                _ => throw new Exception(),
            };
        }

        private Color TypeToColor(EType logType)
        {
            return logType switch
            {
                EType.Error => ErrorColor,
                EType.Warning => WarningColor,
                EType.Log => LogColor,
                EType.Callback => CallbackColor,
                _ => throw new Exception(),
            };
        }

        private void Update()
        {
            foreach (var newLog in m_newLogs)
            {
                _ = m_logSB.Clear();
                var text = Instantiate(LogLinePrefab, LogLinesParent);

                _ = m_logSB.Append(string.Format("{0,12}", newLog.Timestamp.ToString(TimestampFormat)));
                var logType = newLog.LogType.ToString().ToUpper();
                _ = m_logSB.Append(string.Format("{0,5}", logType[..Math.Min(4, logType.Length)]));
                _ = m_logSB.Append(' ');
                _ = m_logSB.Append(newLog.Message);

                text.Set(m_logSB.ToString(), newLog.Details, TypeToColor(newLog.LogType));
            }
            m_newLogs.Clear();
        }
    }
}
