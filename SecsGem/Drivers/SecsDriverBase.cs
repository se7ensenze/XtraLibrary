using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace XtraLibrary.SecsGem.Drivers 
{
    public abstract class SecsDriverBase
    {
        public event PrimarySecsMessageEventHandler ReceivedPrimaryMessage;
        public event SecondarySecsMessageEventHandler ReceivedSecondaryMessage;
        //public event SecondarySecsMessageEventHandler SentMessage;
        public event TraceLogEventHandler TraceLog;
 
        private SmlBuilder m_SmlBuilder;

        private ushort m_DeviceId;
        public ushort DeviceId
        {
            get
            {
                return m_DeviceId;
            }
            set
            {
                m_DeviceId = value;
            }
        }

        private uint m_TransactionId;

        protected SynchronizationContext m_SyncContext;
        private SecsMessageParserBase m_MsgParser;
        
        protected SecsDriverBase(SecsMessageParserBase parser) {
            m_TransactionId = 0;
            m_DeviceId = 0;
            m_SyncContext = SynchronizationContext.Current;
            if (m_SyncContext == null)
            {
                m_SyncContext = new SynchronizationContext();
            }
            m_MsgParser = parser;
            m_SmlBuilder = new SmlBuilder();
        }

        public void Send(SecsMessageBase msg)
        {            
            msg.DeviceId = m_DeviceId;
            bool isPrimary = ((msg.Function & 0x01) == 0x01);

            if (isPrimary) //primary message
            {
                msg.TransactionId = GetNextTransactionId();
                OnSendingPrimaryMessage(msg);
            }
            //should try { } catch {} for error
            byte[] data = m_MsgParser.GetBytes(msg);
            
            SendBytes(data);
            //RaiseEventSend(msg);
            TraceSmlLog(DateTime.Now, msg, TraceLogEventArgs.DirectionType.Sent);
        }

        protected abstract void OnSendingPrimaryMessage(SecsMessageBase msg);
        
        protected abstract void SendBytes(byte[] data);
        
        public abstract void Start();
        public abstract void Stop();
        
        protected uint GetNextTransactionId()
        {
            lock (this)
            {
                if (m_TransactionId == uint.MaxValue)
                {
                    m_TransactionId = 1;
                }
                else
                {
                    m_TransactionId += 1;
                }
            }

            return m_TransactionId;
        }

        #region "Raise Events"

        protected void RaiseEventReceivedPrimaryMessage(SecsMessageBase primary)
        {
            if (ReceivedPrimaryMessage != null)
            {
                m_SyncContext.Post(Post_Event_ReceivedPrimaryMessage, new PrimarySecsMessageEventArgs(primary));
            }
        }

        private void Post_Event_ReceivedPrimaryMessage(object state)
        {

            PrimarySecsMessageEventArgs e = (PrimarySecsMessageEventArgs)state;
            ReceivedPrimaryMessage(this, e);
            
        }

        protected void RaiseEventReceivedSecondaryMessage(SecsMessageBase primary, SecsMessageBase secondary)
        {
            if (ReceivedSecondaryMessage != null)
            {
                m_SyncContext.Post(Post_Event_ReceivedSecondaryMessage, new SecondarySecsMessageEventArgs(primary, secondary));
            }
        }

        private void Post_Event_ReceivedSecondaryMessage(object state)
        {            
            SecondarySecsMessageEventArgs e = (SecondarySecsMessageEventArgs)state;
            ReceivedSecondaryMessage(this, e);            
        }

        protected void RaiseEventTraceLog(DateTime timeStamp, string strSml, TraceLogEventArgs.DirectionType direction)
        {
            if (TraceLog != null)
            {
                m_SyncContext.Post(Post_Evemt_TraceLog, new TraceLogEventArgs(timeStamp, strSml, direction));
            }
        }

        private void Post_Evemt_TraceLog(object state)
        { 
            TraceLogEventArgs e = (TraceLogEventArgs)state;
            TraceLog(this, e);
        }


        #endregion

        private void TraceSmlLog(DateTime timeStamp, SecsMessageBase msg, TraceLogEventArgs.DirectionType direct)
        {
            string strSml = m_SmlBuilder.ToSmlString(msg);
            //keep log to file

            //fire event
            RaiseEventTraceLog(timeStamp, strSml, direct);
        }
    }
}
