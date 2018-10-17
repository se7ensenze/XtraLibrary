using System;
using System.Collections.Generic;

using System.Text;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace XtraLibrary.SecsGem
{
    public class HsmsHost
        :SecsHost 
    {

        public event HsmsStateChangedEventHandler HsmsStateChanged;

        public new SecsIIMessageParser MessageParser
        {
            get
            {
                return (SecsIIMessageParser)m_Parser;
            }
            set
            {
                m_Parser = value;
            }
        }

        public HsmsHost()
            : this(new HsmsParameters(), new SecsIIMessageParser())
        {  
        }

        public HsmsHost(HsmsParameters parameters)
            : this(parameters, new SecsIIMessageParser())
        {            
        }

        //to support single instance of parser for multiple hosts
        public HsmsHost(HsmsParameters parameters, SecsIIMessageParser parser)
            : base(parser)
        {
            m_Parameters = parameters;

            m_T3Hash = new Hashtable();

            m_Timer_T5 = new System.Timers.Timer();
            m_Timer_T5.Elapsed += new System.Timers.ElapsedEventHandler(m_Timer_T5_Elapsed);

            m_Timer_T6 = new System.Timers.Timer();
            m_Timer_T6.Elapsed += new System.Timers.ElapsedEventHandler(m_Timer_T6_Elapsed);

            m_Timer_T7 = new System.Timers.Timer();
            m_Timer_T7.Elapsed += new System.Timers.ElapsedEventHandler(m_Timer_T7_Elapsed);

            m_Timer_T8 = new System.Timers.Timer();
            m_Timer_T8.Elapsed += new System.Timers.ElapsedEventHandler(m_Timer_T8_Elapsed);

            m_Timer_Linktest = new System.Timers.Timer();
            m_Timer_Linktest.Elapsed += new System.Timers.ElapsedEventHandler(m_Timer_Linktest_Elapsed);

            m_Locker = new object();

            m_State = HsmsState.NOT_CONNECTED;

            m_HsmsLogEnabled = false;
        }

        #region "Post Events"

        private void PostEvent_HsmsStateChanged(HsmsState changedState)
        {
            if (HsmsStateChanged != null)
            {
                m_SyncContext.Post((object state) => {
                    HsmsStateChanged(this, new HsmsStateChangedEventArgs(changedState));
                }, null);
            }
        }

        #endregion

        #region "Private Methods"

        private void ChangeHsmsState(HsmsState newState)
        {
            lock (m_Locker)
            {
                if (m_State != newState)
                {
                    m_State = newState;
                    //signal event to base case
                    PostEvent_HsmsStateChanged(newState);
                }
            }
        }

        #endregion

        #region "Abstract methods"

        protected override void ProtectedSend(byte[] data)
        {
            if (m_State != HsmsState.SELECTED)
            {
                
                throw new Exception("HSMS State is not SELECTED");
            }
            SendBytes(data);
        }

        protected override void OnSending(SecsMessageBase msg, bool isPrimary)
        {
            if (isPrimary)
            {
                //make sure the primary message will be register before
                System.Diagnostics.Debug.Print("Send TID:= {0}", msg.TransactionId);
                //equipment will response secondary message
                T3_Timer_Start(msg.TransactionId);
            }
        }

        protected override void OnReceiving(SecsMessageBase msg, bool isSecondary)
        {
            if (isSecondary)
            {
                System.Diagnostics.Debug.Print("Recv TID:= {0}", msg.TransactionId);
                T3_Timer_Stop(msg.TransactionId);
            }
        }

        public override void Connect()
        {
            T5_Timer_Stop();
            T6_Timer_Stop();
            T7_Timer_Stop();
            LinkTest_Timer_Stop();

            ChangeHsmsState(HsmsState.NOT_CONNECTED);
            
            if (m_Parameters.Mode == HsmsConnectProcedure.ACTIVE)
            {
                HsmsDriver_BeginConnect();
                //start T5
                T5_Timer_Start();
            }
            else
            {
                HsmsDriver_BeginAccept();
            }
        }

        public override void Disconnect()
        {

            T5_Timer_Stop();
            T6_Timer_Stop();
            T7_Timer_Stop();
            LinkTest_Timer_Stop();
            
            if (m_SocketWorker != null)
            {
                if (m_SocketWorker.Connected)
                {
                    Send_Request_ControlMessage(SType.SeparateReq);
                }
                m_SocketWorker.Close();
                m_SocketWorker = null;
            }

            if (m_SocketListener != null)
            {
                m_SocketListener.Close();
                m_SocketListener = null;
            }

            ChangeHsmsState(HsmsState.NOT_CONNECTED);
        }

        #endregion

        #region "HSMS Driver"
        
        #region "Variables"

        private HsmsState m_State;

        private System.Timers.Timer m_Timer_T5;
        private System.Timers.Timer m_Timer_T6;
        private System.Timers.Timer m_Timer_T7;
        private System.Timers.Timer m_Timer_T8;
        private System.Timers.Timer m_Timer_Linktest;

        protected HsmsParameters m_Parameters;
       
        private Hashtable m_T3Hash;

        private Socket m_SocketWorker;
        private Socket m_SocketListener;

        private object m_Locker;

        #endregion

        #region "Properties"

        public HsmsState State
        {
            get {
                return m_State;
            }
        }
           
        #endregion

        #region "Private class"

        private class SocketStateHolder
        {
            public Socket Socket { get; set; }
            public byte[] Header { get; set; }
            public byte[] Body { get; set; }
            public int RecvStartIndex { get; set; }
        }

        #endregion

        #region "Timers"

        void m_Timer_T8_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            WriteHsmsLog(string.Format("T8 Timedout [{0} sec]", m_Parameters.T8_Interval));
            m_Timer_T8.Stop();
            //do it job
            ChangeHsmsState(HsmsState.NOT_CONNECTED);
            HsmsDriver_BeginAccept();
        }

        private void T8_Timer_Start()
        {
            m_Timer_T8.Interval = m_Parameters.T8_Interval * 1000;
            m_Timer_T8.Start();
        }

        private void T8_Timer_Stop()
        {
            m_Timer_T8.Stop();
        }

        private void T6_Timer_Start()
        {
            m_Timer_T6.Interval = m_Parameters.T6_Interval * 1000;
            m_Timer_T6.Start();
        }

        private void T6_Timer_Stop() {
            m_Timer_T6.Stop();
        }

        void m_Timer_T6_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            WriteHsmsLog(string.Format("T6 Timedout [{0} sec]", m_Parameters.T6_Interval));
            T5_Timer_Start();
        }

        void m_Timer_Linktest_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            m_Timer_Linktest.Stop();
            SendLinkTestRequest();
            T6_Timer_Start();
        }

        void m_Timer_T7_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            WriteHsmsLog(string.Format("T7 Timedout [{0} sec]", m_Parameters.T7_Interval));
            m_Timer_T7.Stop();
            ChangeHsmsState(HsmsState.NOT_CONNECTED);
            HsmsDriver_BeginAccept();
        }

        void T3_Timer_Start(uint tid)
        {
            lock (m_Locker)
            {
                T3Timer timer_t3 = new T3Timer(tid);
                timer_t3.Interval = m_Parameters.T3_Interval * 1000;
                timer_t3.Elapsed += new System.Timers.ElapsedEventHandler(T3_Timer_Elapsed);
                timer_t3.Start();
                m_T3Hash.Add(tid, timer_t3);
            }
        }

        void T3_Timer_Stop(uint tid)
        {
            lock (m_Locker)
            {
                if (m_T3Hash.ContainsKey(tid))
                {
                    T3Timer timer_t3 = (T3Timer)m_T3Hash[tid];
                    timer_t3.Stop();
                    m_T3Hash.Remove(tid);
                }
            }
        }

        void T3_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            T3Timer timer_t3 = (T3Timer)sender;
            timer_t3.Stop();
                        
            WriteHsmsLog(string.Format("T3 Timedout [TID:{0}, Interval:{1} sec]", 
                timer_t3.TransactionId, m_Parameters.T3_Interval));
            lock (m_Locker)
            {
                if (m_T3Hash.ContainsKey(timer_t3.TransactionId))
                {
                    m_T3Hash.Remove(timer_t3.TransactionId);
                }
            }
            //notifi base class trasaction timed out
            base.TransactionTimeout(timer_t3.TransactionId);

            timer_t3.Dispose();
        }

        void m_Timer_T5_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            WriteHsmsLog(string.Format("T5 Timedout [{0} sec]", m_Parameters.T5_Interval));
            m_Timer_T5.Stop();
            //try to re-connect to ACTIVE
            HsmsDriver_BeginConnect();
        }

        private void T5_Timer_Start() {        
            m_Timer_T5.Interval = m_Parameters.T5_Interval * 1000;
            m_Timer_T5.Start();
        }

        private void T5_Timer_Stop()
        {
            m_Timer_T5.Stop();
        }

        private void T7_Timer_Start() {
            m_Timer_T7.Interval = m_Parameters.T7_Interval * 1000;
            m_Timer_T7.Start();
        }

        private void T7_Timer_Stop() {
            m_Timer_T7.Stop();
        }

        private void LinkTest_Timer_Start()
        {
            if (m_Parameters.LinktestEnabled)
            {
                m_Timer_Linktest.Interval = m_Parameters.Linktest_Interval * 1000;
                m_Timer_Linktest.Start();
            }
        }

        private void LinkTest_Timer_Stop()
        {
            m_Timer_Linktest.Stop();
        }

        #endregion
        
        #region "ACTIVE"

        int m_HsmsIsConnecting = 0;
        
        private void HsmsDriver_BeginConnect()
        {
            //prevent HsmsDriver_BeginConnect to be called again while connecting
            //if original value == 0 the go
            if (Interlocked.Exchange(ref m_HsmsIsConnecting, 1) != 0) 
            {
                return;
            }

            IPAddress ip = IPAddress.Parse(m_Parameters.IPAddress);
                        
            if (m_SocketWorker != null && m_SocketWorker.Connected)
            {
                m_SocketWorker.Close();
            }
            m_SocketWorker = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            m_SocketWorker.BeginConnect(new IPEndPoint(ip, m_Parameters.PortNo), HsmsDriver_BeginConnectCallback, m_SocketWorker);            
       
        }
                
        private void HsmsDriver_BeginConnectCallback(IAsyncResult ar)
        {

            Interlocked.Exchange(ref m_HsmsIsConnecting, 0);

            Socket sck = (Socket)ar.AsyncState;

            if (sck.Connected)
            {
                sck.EndConnect(ar);
                //change state to NOT_SELECTED
                ChangeHsmsState(HsmsState.NOT_SELECTED);
                //stop T5
                T5_Timer_Stop();
                //wait message
                WaitForMessage_Header(sck);
                //send Select.Req
                SendSelectRequest();
                //start T6
                T6_Timer_Start();
            }
            else
            {
                //reset state to NOT_CONNECTED
                ChangeHsmsState(HsmsState.NOT_CONNECTED);
                //start T5 for retry
                T5_Timer_Start();
            }
        }

        #endregion

        #region "PASSIVE"

        private int m_HsmsAccepting = 0;

        private void HsmsDriver_BeginAccept() {

            //lock this function
            //check if the function is already working by other thread
            //https://docs.microsoft.com/en-us/dotnet/api/system.threading.interlocked.exchange?view=netframework-4.7.2
            //Sets a 32-bit signed integer to a specified value and returns the original value, as an atomic operation.
            if (Interlocked.Exchange(ref m_HsmsAccepting, 1) != 0)
            {
                //try to set 1 to variable
                //if default value is 1, it's mean this function is being execute
                return;
            }

            //close socket
            if (m_SocketWorker != null && m_SocketWorker.Connected)
            {
                m_SocketWorker.Close();
            }

            //start listening if not listening
            if (m_SocketListener == null || !m_SocketListener.IsBound)
            {
                m_SocketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_SocketListener.Bind(new IPEndPoint(System.Net.IPAddress.Any, m_Parameters.PortNo)); //will change IsBound to "true"
                m_SocketListener.Listen(0);
            }

            try
            {
                //accept new socket connection
                m_SocketListener.BeginAccept(HsmsDriver_BeginAcceptCallback, m_SocketListener);
            }
            catch (SocketException se)
            {
                //this port is already opened
                if (se.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    throw se;
                }
            }
            finally
            {
                //clear locking
                m_HsmsAccepting = 0;
            }
        }

        private void HsmsDriver_BeginAcceptCallback(IAsyncResult ar)
        {
            Interlocked.Exchange(ref m_HsmsAccepting, 0);

            try
            {
                m_SocketWorker = m_SocketListener.EndAccept(ar);
            }
            catch(SocketException)
            {
                ChangeHsmsState(HsmsState.NOT_CONNECTED);
                return;
            }
            //hsms state > connect but not select
            ChangeHsmsState(HsmsState.NOT_SELECTED);
            //start T7
            T7_Timer_Start();
            //wait Select.Req   
            WaitForMessage_Header(m_SocketWorker);
        }
        
        #endregion

        #region "Control Message"

        private void SendLinkTestRequest()
        {
            Send_Request_ControlMessage(SType.LinktestReq);
        }

        private void SendLinkTestResponse(byte[] data)
        {
            Send_Response_ControlMessage(data);
        }

        private void Send_Response_ControlMessage(byte[] data)
        {
            byte[] sendBuff = new byte[data.Length];
            Array.Copy(data, sendBuff, data.Length);
            sendBuff[9] = (byte)(sendBuff[9] + 1);            
            SendBytes(sendBuff);
        }

        private byte[] CreateControlMessage(SType sType)
        {
            return new byte[] { 0x00, 0x00, 0x00, 0x0A, 
                0xFF, 0xFF, //device
                0x00, 0x00, //stream , function
                0x00, //pType
                (byte)sType, //sType
                0x00, 0x00, 0x00, 0x00  //transaction 
            };
        }

        private void SendSelectRequest()
        {
            Send_Request_ControlMessage(SType.SelectReq);
        }

        private void SendSelectReponse(byte[] data)
        {
            Send_Response_ControlMessage(data);
        }

        private void SendSeparateRequest()
        {
            Send_Request_ControlMessage(SType.SeparateReq);
        }

        private void Send_Request_ControlMessage(SType sType)
        {
            byte[] msg = CreateControlMessage(sType);
            byte[] tranId = BitConverter.GetBytes(this.GetNextTransactionId());
            Array.Reverse(tranId);
            Array.Copy(tranId, 0, msg, 10, 4);
            SendBytes(msg);
        }

        #endregion

        #region "Receiving Bytes Message"

        private void WaitForMessage_Header(Socket sck)
        {
            byte[] header = new byte[14];
            WaitForMessage_Header(sck, header, 0);
        }

        private void WaitForMessage_Header(Socket sck, byte[] header, int headerStartIndex)
        {
            //was stop
            if (sck == null || m_State == HsmsState.NOT_CONNECTED)
            {
                return;
            }

            if (sck.Connected)
            {

                SocketStateHolder holder = new SocketStateHolder();
                holder.Socket = sck;
                holder.Header = header;
                holder.RecvStartIndex = headerStartIndex;

                SocketError se;
                              
                try
                {
                    sck.BeginReceive(header, headerStartIndex, header.Length - headerStartIndex,
                        SocketFlags.None, out se, WaitForMessage_Header_Callback, holder);
                }
                catch (ObjectDisposedException)
                {
                    ContinueMakeConnection();
                }              
                
            }
        }

        private void ContinueMakeConnection()
        {
            ChangeHsmsState(HsmsState.NOT_CONNECTED);

            T6_Timer_Stop();
            T7_Timer_Stop();
            T8_Timer_Stop();
            LinkTest_Timer_Stop();
            //try to connect
            if (m_Parameters.Mode == HsmsConnectProcedure.ACTIVE)
            {
                T5_Timer_Start();
            }
            else
            {
                HsmsDriver_BeginAccept();
            }
        }

        private void WaitForMessage_Header_Callback(IAsyncResult ar)
        {
            SocketStateHolder holder = (SocketStateHolder)ar.AsyncState;

            Socket sck = holder.Socket;            

            if (!sck.Connected)
            {
                ContinueMakeConnection();
                return;
            }

            byte[] header = holder.Header;

            int byteCount = 0;

            try
            {
                byteCount = sck.EndReceive(ar);
            }
            catch (SocketException se)
            {

                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    ContinueMakeConnection();
                }
                return;
            }

            if (byteCount == 0) {
                return;
            }

            holder.RecvStartIndex += byteCount;               
            
            if (holder.RecvStartIndex == header.Length)
            {
                SType sType = (SType)header[9];

                bool needBody = false;
                uint bodyByteCount = 0;

                if (sType == SType.Message)
                {
                    byte[] lbb = new byte[4];
                    Array.Copy(header, 0, lbb, 0, 4);
                    Array.Reverse(lbb);
                    bodyByteCount = BitConverter.ToUInt32(lbb, 0) - 10; //exclude header
                    needBody = (bodyByteCount > 0); //header
                }
                
                if (needBody)
                {
                    byte[] body = new byte[bodyByteCount];
                    WaitForMessage_Body(sck, header, body, 0);
                }
                else
                {                    
                    //keep hsms log file
                    WriteHsmsLog(header, DirectionType.Recv);
                    //case of completed message and control message
                    switch (sType)
                    {
                        case SType.Message:
                            ProcessSecsMessageBytes(header);
                            break;
                        case SType.SelectReq:
                            if (m_Parameters.Mode == HsmsConnectProcedure.PASSIVE)
                            {
                                ChangeHsmsState(HsmsState.SELECTED);
                                //stop T7
                                T7_Timer_Stop();
                                //Select.Req relate with T5
                                SendSelectReponse(header);
                            }
                            break;
                        case SType.SelectResp:
                            if (m_Parameters.Mode == HsmsConnectProcedure.ACTIVE)
                            {
                                ChangeHsmsState(HsmsState.SELECTED);
                                //T6 timer
                                T6_Timer_Stop();
                                //start link test timer
                                LinkTest_Timer_Start();
                            }
                            break;
                        case SType.LinktestReq:
                            SendLinkTestResponse(header);
                            break;
                        case SType.LinktestResp:
                            //T6 timer stop
                            T6_Timer_Stop();
                            //l
                            LinkTest_Timer_Start();
                            break;
                        case SType.SeparateReq:
                            ContinueMakeConnection();
                            break;
                        default:
                            //unknow
                            break;
                    }

                    WaitForMessage_Header(sck);
                }
            }
            else
            {
                WaitForMessage_Header(sck, header, holder.RecvStartIndex);
            }        

            holder.Socket = null;
            holder.Header = null;
            holder.Body = null;
            holder = null;
        }


        
        private void WaitForMessage_Body(Socket sck, byte[] header, byte[] body, int bodyStartIndex)
        {
            if (sck.Connected)
            {

                SocketError se;
                SocketStateHolder holder = new SocketStateHolder();
                holder.Socket = sck;
                holder.Header = header;
                holder.Body = body;
                holder.RecvStartIndex = bodyStartIndex;

                sck.BeginReceive(body, bodyStartIndex, body.Length - bodyStartIndex, SocketFlags.None,
                   out se, WaitForMessage_Body_Callback, holder);
            }
        }

        private void WaitForMessage_Body_Callback(IAsyncResult ar)
        {
            SocketStateHolder holder = (SocketStateHolder)ar.AsyncState;
            Socket sck = holder.Socket;

            if (!sck.Connected)
            {
                return;
            }

            byte[] header = holder.Header;
            byte[] body = holder.Body;
            
            //***********************

            int rcvByteCount = 0;

            try
            {
                rcvByteCount = sck.EndReceive(ar);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    ContinueMakeConnection();
                }
                return;
            }

            holder.RecvStartIndex += rcvByteCount;

            if (holder.RecvStartIndex == body.Length)
            {                
                byte[] data = new byte[header.Length + body.Length];
                Array.Copy(header, 0, data, 0, header.Length);
                Array.Copy(body, 0, data, header.Length, body.Length);
                //keep log
                WriteHsmsLog(data,  DirectionType.Recv);
                //process secs message
                ProcessSecsMessageBytes(data);
                //continue to next message
                WaitForMessage_Header(holder.Socket);
            }
            else
            {
                WaitForMessage_Body(holder.Socket, header, body, holder.RecvStartIndex);
            }

        }

        #endregion

        #region "Sending Bytes Message"

        private void SendBytes(byte[] data)
        {
            WriteHsmsLog(data, DirectionType.Sent);
                        
            if (m_SocketWorker.Connected)
            {
                SocketError se = SocketError.Success;
                m_SocketWorker.BeginSend(data, 0, data.Length, SocketFlags.None, out se, SendBytesCallback, m_SocketWorker);
            }
        }    

        private void SendBytesCallback(IAsyncResult ar)
        {
            Socket sck = (Socket)ar.AsyncState;

            if (sck.Connected)
            {
                try
                {
                    int sentByteCount = sck.EndSend(ar);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print("{0}: SendBytesCallback Failed was due to Exception := {1}",
                    m_Parameters.Mode, ex.Message);
                }

            }
            else
            {
                System.Diagnostics.Debug.Print("{0}: SendBytes Failed was due to Socket.Connected = {1}",
                    m_Parameters.Mode, sck.Connected);
            }
        }

        #endregion
        
        #endregion

        #region "Logging"

        private void WriteHsmsLog(byte[] secsBytes, DirectionType direction)
        {            
             WriteHsmsLog("[" + direction.ToString() + "]:" + GetHexString(secsBytes)); 
        }

        private void WriteHsmsLog(string message)
        {
            if (!m_HsmsLogEnabled)
            {
                return;
            }

            lock (m_Locker)
            {
                string fileName = Path.Combine(m_LogDirectory, "HSMS_" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    writer.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff ") + message);
                }
            }
        }

        #endregion

        #region "Public properties"

        private bool m_HsmsLogEnabled;

        public bool HsmsLogEnabled
        {
            get
            {
                return m_HsmsLogEnabled;
            }
            set
            {
                m_HsmsLogEnabled = value;
            }
        }

        public HsmsParameters Parameters
        {
            get {
                return m_Parameters;
            }
        }

        #endregion

        #region "For Debug"

        private string GetHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in data)
            {
                if (sb.Length == 0)
                {
                    sb.Append(b.ToString("X2"));
                }
                else
                {
                    sb.Append(" " + b.ToString("X2"));
                }
            }

            return sb.ToString();
        }

        #endregion

    }
}
