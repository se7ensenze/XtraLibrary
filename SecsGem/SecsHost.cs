using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace XtraLibrary.SecsGem
{
    public abstract class SecsHost
    {

        #region "Extended properties"

        //************************************
        //Added By Tanapat S. since 2017-09-21
        //************************************
        /// <summary>
        /// For keep Machine name ... requested by Mr.Prasarn 
        /// </summary>
        public string Name { get; set; }
        //************************************

        #endregion

        #region "Variables"

        private object m_Locker;

        private SecsItemManager m_ItemManager;
        private Dictionary<uint, SecsMessageBase> m_SecsTransaction;
        
        private ushort m_DeviceId;
        private uint m_TransactionIdSeed;
        protected SynchronizationContext m_SyncContext;

        private bool m_SecsLogEnabled;
        protected string m_LogDirectory;

        protected SecsMessageParserBase m_Parser;

        #endregion

        #region "Events"

        public event TraceLogEventHandler TracedSmlLog;
        public event SecsErrorNotificationEventHandler ErrorNotification;
        public event PrimarySecsMessageEventHandler ReceivedPrimaryMessage;
        public event SecondarySecsMessageEventHandler ReceivedSecondaryMessage;
        public event ConversionErrorEventHandler ConversionErrored;

        #endregion

        #region "Properties"

        public ushort DeviceId
        {
            get {
                return m_DeviceId;
            }
            set {
                m_DeviceId = value;
            }
        }

        public bool SecsLogEnabled
        {
            get {
                return m_SecsLogEnabled;
            }
            set
            {
                m_SecsLogEnabled = value;
            }
        }

        public string LogDirectory
        {
            get {
                return m_LogDirectory;
            }
            set
            {
                m_LogDirectory = value;
            }
        }

        public SecsMessageParserBase MessageParser
        {
            get {
                return m_Parser;
            }
        }

        #endregion

        protected SecsHost(SecsMessageParserBase parser)
        {
            m_Locker = new object();
            m_Parser = parser;
            m_ItemManager = new SecsItemManager();
            m_DeviceId = 0;
            m_SecsTransaction = new Dictionary<uint, SecsMessageBase>();
            m_TransactionIdSeed = 0;

            m_SyncContext = SynchronizationContext.Current;
            if (m_SyncContext == null)
            {
                m_SyncContext = new SynchronizationContext();
            }

            m_SecsLogEnabled = true;
            System.IO.FileInfo fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            m_LogDirectory = fi.DirectoryName;
        }

        #region "Logging"

        private void TraceSmlLog(SecsMessageBase msg, DirectionType direction)
        { 
            DateTime time = DateTime.Now;
            //conver to SML
            string strSml = SmlBuilder.ToSmlString(msg);
            //create log message
            string logMsg = String.Format("{0:yyyy/MM/dd HH:mm:ss.fff} [{1}] " +
                        Environment.NewLine + "{2}", time, direction, strSml);

            //keep to text file
            if (m_SecsLogEnabled)
            {
                WritSmlLog(logMsg);               
            }
            //raise event
            PostEvent_TracedSmlLog(new TraceLogEventArgs(time, strSml, direction, logMsg));
        }

        private void WritSmlLog(string logMsg)
        {        

            lock (m_Locker)
            {
                string fileName = Path.Combine(m_LogDirectory, "SECS_" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                using (StreamWriter writer = new StreamWriter(fileName, true))
                {

                    writer.WriteLine(logMsg);
                }
            }
        }

        #endregion

        #region "Post Events"

        private void PostEvent_ErrorNotification(SecsErrorNotificationEventArgs e)
        {
            if (ErrorNotification != null)
            {
                m_SyncContext.Post((object state) =>
                {
                    ErrorNotification(this, (SecsErrorNotificationEventArgs)state);
                }, e);
            }
        }

        private void PostEvent_TracedSmlLog(TraceLogEventArgs e)
        {
            if (TracedSmlLog != null)
            {
                m_SyncContext.Post((object state) =>
                {

                    TracedSmlLog(this, (TraceLogEventArgs)state);

                }, e);
            }            
        }

        private void PostEvent_ReceivedPrimaryMessage(PrimarySecsMessageEventArgs e)
        {
            if (ReceivedPrimaryMessage != null)
            {
                m_SyncContext.Post((object state) =>
                {
                    ReceivedPrimaryMessage(this, e);
                }, null);
            }
        }

        private void PostEvent_ReceivedSecondaryMessage(SecondarySecsMessageEventArgs e)
        {
            if (ReceivedSecondaryMessage != null)
            {
                m_SyncContext.Post((object state) =>
                {
                    ReceivedSecondaryMessage(this, e);
                }, null);
            }
        }

        private void PoseEvent_ConversionErrored(ConversionErrorEventArgs e) {
            if (ConversionErrored != null)
            {
                m_SyncContext.Post((object state) =>
                {
                    ConversionErrored(this, e);
                }, null);
            }
        }

        #endregion

        #region "Abstract methods"

        public abstract void Connect();
        public abstract void Disconnect();

        //Sending ...
        protected virtual void OnSending(SecsMessageBase msg, bool isPrimary) { }
        //protected virtual void OnSent(SecsMessageBase msg) { }
        protected abstract void ProtectedSend(byte[] data);
        
        //Receiving
        protected virtual void OnReceiving(SecsMessageBase msg, bool isSecondary) { }
        protected virtual void OnReceived(SecsMessageBase msg) { }
        
        #endregion

        #region "SECS MESSAGE BUILD"

        #region "Request message"


        #endregion

        #region "Reply message"

        //Message Fault — A Message Fault occurs when the equipment receives a message 
        //which it cannot process because of a fault that arises from the content, 
        //context, or length of the message
        public void Reply_AbortMessage(SecsMessageBase msg)
        {
            SecsMessage sm = new SecsMessage(9, 0, false);
            sm.TransactionId = msg.TransactionId;
            Send(sm);
        }

        #endregion         

        #endregion

        #region "Public methods"

        /// <summary>
        /// S1F15R	Request OFF-LINE
        /// </summary>
        public void Request_OFFLINE()
        {
            SecsMessage sm = new SecsMessage(1, 15, true);
            Send(sm);
        }

        /// <summary>
        /// S1F17R	Request ON-LINE
        /// </summary>
        public void Request_ONLINE()
        {
            SecsMessage sm = new SecsMessage(1, 17, true);
            Send(sm);
        }

        /// <summary>
        /// S1F13R	Establish Communications Request
        /// </summary>
        public void EstablishCommunicationsRequest()
        {
            SecsMessage sm = new SecsMessage(1, 13, true);
            sm.Items.Add(new SecsItemList("L0"));
            Send(sm);
        }

        public void Send(SecsMessageBase msg)
        {
            msg.DeviceId = m_DeviceId;

            bool isPrimary = msg.NeedReply && ((msg.Function & 0x01) == 0x01);

            if (isPrimary)
            {
                //get new transaction id
                msg.TransactionId = GetNextTransactionId();
                //register T3     
                lock (m_Locker)
                {
                    if (m_SecsTransaction.ContainsKey(msg.TransactionId))
                    {
                        m_SecsTransaction.Remove(msg.TransactionId);
                    }
                    m_SecsTransaction.Add(msg.TransactionId, msg);
                }
            }


           //TraceSmlLog(msg, DirectionType.Sent);
            OnSending(msg, isPrimary);
            byte[] data = m_Parser.GetBytes(msg);
            ProtectedSend(data);
            //OnSent(msg);
         TraceSmlLog(msg, DirectionType.Sent);        //000783 Change Position, Send without Error then Log Show
        }

        public void Reply(SecsMessageBase primary, SecsMessageBase secondaryMessage)
        {
            secondaryMessage.TransactionId = primary.TransactionId;
            Send(secondaryMessage);
        }

        //public void DefinedReport()
        //{ 
            
        //}

        //public void DeleteAllReport()
        //{ 
        
        //}

        //public void LinkReport()
        //{ 
            
        //}

        //public void EnabledReport()
        //{ 
            
        //}

        #endregion

        protected uint GetNextTransactionId()
        {
            uint ret = 0;

            lock (m_Locker)
            {
                if (m_TransactionIdSeed == uint.MaxValue)
                {
                    m_TransactionIdSeed = 1;
                }
                else
                {
                    m_TransactionIdSeed += 1;
                }

                ret = m_TransactionIdSeed;
            }

            return ret;
        }

        /// <summary>
        /// This function is called from inherited class
        /// </summary>
        /// <param name="data">SecsMessage in byte[]</param>
        protected void ProcessSecsMessageBytes(byte[] data)
        {
            SecsMessageBase msg = null;
            try
            {
                msg = m_Parser.ToSecsMessage(data);
            }
            catch (Exception ex)
            {
                //fire out event conversion error
                PoseEvent_ConversionErrored(new ConversionErrorEventArgs(ex, data));
                return;
            }

            TraceSmlLog(msg, DirectionType.Recv);

            bool isSecondary = (msg.Function & 0x01) == 0x00;

            OnReceiving(msg, isSecondary);
            //should put some flag for example :Cancel or abort
            OnReceived(msg);

            if (isSecondary)
            {
                SecsMessageBase priMsg = null;

                lock (m_Locker)
                {
                    if (m_SecsTransaction.ContainsKey(msg.TransactionId))
                    {
                        //get primary message
                        priMsg = m_SecsTransaction[msg.TransactionId];
                        //unregister transaction
                        m_SecsTransaction.Remove(msg.TransactionId);
                    }
                }

                if (priMsg != null)
                {
                    if (msg.Function == 0)
                    {
                        //primary transaction was abort
                        AbortPrimaryTransaction(msg);
                    }
                    else
                    {
                        //should post as asynchronouse
                        PostEvent_ReceivedSecondaryMessage(new SecondarySecsMessageEventArgs(priMsg, msg));
                    }
                }
                else
                {
                    AbortUnknowTransaction(msg);
                }

            }
            else
            {
                //should post as asynchronouse                
                PostEvent_ReceivedPrimaryMessage(new PrimarySecsMessageEventArgs(msg));                
            }   
            
        }       

        private void AbortPrimaryTransaction(SecsMessageBase priMsg)
        {
            PostEvent_ErrorNotification(new SecsErrorNotificationEventArgs("Transaction Aborted", priMsg));
        }

        private void AbortUnknowTransaction(SecsMessageBase msg)
        {
            PostEvent_ErrorNotification(new SecsErrorNotificationEventArgs("Unknow Transaction", msg));
        }
                
        protected void TransactionTimeout(uint transactionId)
        {   
            SecsMessageBase msg = null;
            lock (m_Locker)
            {
                if (m_SecsTransaction.ContainsKey(transactionId))
                {
                    msg = m_SecsTransaction[transactionId];
                    m_SecsTransaction.Remove(transactionId);
                }
                else
                {
                    //unknow transaction
                    return;
                }
            }

            if (msg != null)
            {
                 PostEvent_ErrorNotification(new SecsErrorNotificationEventArgs("Transaction Timeout", msg));
            }
            else
            { 
                 PostEvent_ErrorNotification(new SecsErrorNotificationEventArgs("Transaction Timeout", transactionId));
            }
        }
                
    }
}
