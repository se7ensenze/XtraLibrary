using System;

namespace XtraLibrary.SecsGem
{
    public sealed class PrimarySecsMessageEventArgs
    {

        private SecsMessageBase m_Primary;
        public SecsMessageBase Primary
        {
            get
            {
                return m_Primary;
            }
        }

        internal PrimarySecsMessageEventArgs(SecsMessageBase priMsg)
        {
            m_Primary = priMsg;
        }
    }

    public delegate void PrimarySecsMessageEventHandler(object sender, PrimarySecsMessageEventArgs e);

    public sealed class SecondarySecsMessageEventArgs
    {
        private SecsMessageBase m_Primary;
        public SecsMessageBase Primary
        {
            get
            {
                return m_Primary;
            }
        }

        private SecsMessageBase m_Secondary;
        public SecsMessageBase Secondary
        {
            get
            {
                return m_Secondary;
            }
        }

        internal SecondarySecsMessageEventArgs(SecsMessageBase priMsg, SecsMessageBase secMsg)
        {
            m_Primary = priMsg;
            m_Secondary = secMsg;
        }
    }

    public delegate void SecondarySecsMessageEventHandler(object sender, SecondarySecsMessageEventArgs e);
    
    public sealed class TraceLogEventArgs
    {
        private string m_SML;
        public string SML
        {
            get
            {
                return m_SML;
            }
        }

        private DirectionType m_Direction;
        public DirectionType Direction
        {
            get {
                return m_Direction;
            }
        }

        private DateTime m_TimeStamp;

        public DateTime TimeStamp
        {
            get {
                return m_TimeStamp;
            }
        }

        private string m_LogMessage;
        public string LogMessage
        {
            get {
                return m_LogMessage;
            }
        }

        internal TraceLogEventArgs(DateTime timeStamp, string strSml, DirectionType direct, string logMsg)
        {
            m_TimeStamp = timeStamp;
            m_SML = strSml;
            m_Direction = direct;
            m_LogMessage = logMsg;
        }
        
    }

    public delegate void TraceLogEventHandler(object sender, TraceLogEventArgs e);

    public sealed class EventReportReceivedEventArgs
    {
        internal EventReportReceivedEventArgs()
        {

        }
    }

    public delegate void EventReportReceivedEventHandler(object sender, EventReportReceivedEventArgs e);

    public sealed class SecsErrorNotificationEventArgs
    {

        private string m_Message;

        public string Message
        {
            get {
                return m_Message;
            }
        }

        private uint m_TransactionId;

        public uint TransactionId
        {
            get {
                return m_TransactionId;
            }
        }

        private SecsMessageBase m_Source;

        public SecsMessageBase Source
        {
            get {
                return m_Source;
            }
        }

        internal SecsErrorNotificationEventArgs(string errMessage, SecsMessageBase msg)
            :this(errMessage, msg.TransactionId)
        {
            m_Source = msg;
        }

        internal SecsErrorNotificationEventArgs(string errMessage, uint tid)
        {
            m_Message = errMessage;
            m_TransactionId = tid;
        }
    }

    public delegate void SecsErrorNotificationEventHandler(object sender, SecsErrorNotificationEventArgs e);

    //public class AlarmReportReceivedEventArgs
    //{
    //    private SecsMessageBase m_S5F1;
    //    public SecsMessageBase S5F1
    //    {
    //        get {
    //            return m_S5F1;
    //        }
    //    }

    //    internal AlarmReportReceivedEventArgs(SecsMessageBase msg)
    //    {
    //        m_S5F1 = msg;
    //    }
    //}

    //public delegate void AlarmReportReceivedEventHandler(object sender, AlarmReportReceivedEventArgs e);

    public class ConversionErrorEventArgs
    {
        private Exception m_Exception;
        public Exception Exception
        {
            get {
                return m_Exception;
            }
        }

        private byte[] m_Data;
        public byte[] Data
        {
            get {
                return m_Data;
            }
        }

        internal ConversionErrorEventArgs(Exception ex, byte[] data)
        {
            m_Exception = ex;
            m_Data = data;
        }
    }

    public delegate void ConversionErrorEventHandler(object sender, ConversionErrorEventArgs e);

    public class HsmsStateChangedEventArgs
    {
        private HsmsState m_State;

        public HsmsState State
        {
            get {
                return m_State;
            }
        }

        internal HsmsStateChangedEventArgs(HsmsState newState)
        {
            m_State = newState;
        }
    }

    public delegate void HsmsStateChangedEventHandler(object sender, HsmsStateChangedEventArgs e);

    public class SecsIConnectionChangedEventArgs
    {
        private bool m_Connected;
        public bool Connected
        {
            get {
                return m_Connected;
            }
        }

        internal SecsIConnectionChangedEventArgs(bool isConnected)
        {
            m_Connected = isConnected;
        }
    }

    public delegate void SecsIConnectionChangedEventHandler(object sender, SecsIConnectionChangedEventArgs e);
}