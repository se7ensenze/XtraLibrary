using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections;

namespace XtraLibrary.SecsGem.Drivers
{
    public class HsmsDriver
        :SecsDriverBase 
    {

        private SecsIIMessageParser m_MsgPaser;

        private HsmsConnectProcedure m_ConnectProcedure;
        private IPAddress m_IPAddress;
        private int m_PortNo;
        private HsmsState m_State;

        private int m_T3_Interval;
        private int m_T5_Interval;
        private int m_T6_Interval;
        private int m_T7_Interval;
        private int m_T8_Interval;
        private int m_Linktest_Interval;

        private System.Timers.Timer m_Timer_T5;
        private System.Timers.Timer m_Timer_T6;
        private System.Timers.Timer m_Timer_T7;
        private System.Timers.Timer m_Timer_T8;
        private System.Timers.Timer m_Timer_Linktest;

        public event EventHandler OnSelected; 
        
        public HsmsState State
        {
            get {
                return m_State;
            }
        }

        public int T3_Interval
        {
            get {
                return m_T3_Interval;
            }
            set {
                if (value < 1 || value > 120)
                {
                    throw new Exception("Value is out of range 1 - 120 secs");
                }
                m_T3_Interval = value;
            }
        }

        public int T5_Interval
        {
            get
            {
                return m_T5_Interval;
            }
            set
            {
                if (value < 1 || value > 240)
                {
                    throw new Exception("Value is out of range 1 - 240 secs");
                }
                m_T5_Interval = value;
            }
        }

        public int T6_Interval
        {
            get
            {
                return m_T6_Interval;
            }
            set
            {
                if (value < 1 || value > 240)
                {
                    throw new Exception("Value is out of range 1 - 240 secs");
                }
                m_T6_Interval = value;
            }
        }

        public int T7_Interval
        {
            get
            {
                return m_T7_Interval;
            }
            set
            {
                if (value < 1 || value > 240)
                {
                    throw new Exception("Value is out of range 1 - 240 secs");
                }
                m_T7_Interval = value;
            }
        }

        public int T8_Interval
        {
            get
            {
                return m_T8_Interval;
            }
            set
            {
                if (value < 1 || value > 120)
                {
                    throw new Exception("Value is out of range 1 - 240 secs");
                }
                m_T8_Interval = value;
            }
        }
        public int Linktest_Interval
        {
            get
            {
                return m_Linktest_Interval;
            }
            set
            {
                if (value < 1 || value > 900)
                {
                    throw new Exception("Value is out of range 1 - 900 secs");
                }
                m_Linktest_Interval = value;
            }
        }


        #region "Private class"

        //private class T3Timer
        //    : System.Timers.Timer
        //{

        //    private SecsMessageBase m_Msg;
        //    public SecsMessageBase Message
        //    {
        //        get {
        //            return m_Msg;
        //        }
        //    }

        //    public uint TransactionId { 
        //       get { 
        //            return m_Msg.TransactionId; 
        //       } 
        //    }

        //    internal T3Timer(SecsMessageBase msg)
        //    {
        //        m_Msg = msg;
        //    }

        //    ~T3Timer() {
        //        m_Msg = null;
        //    }
        //}

        private class SocketStateHolder
        {
            public Socket Socket { get; set; }
            public byte[] Header { get; set; }
            public byte[] Body { get; set; }
            public int RecvStartIndex { get; set; }
        }

        #endregion

        private Hashtable m_T3Hash;

        public HsmsDriver(HsmsConnectProcedure proc, string ip, int portNo, SecsIIMessageParser parser)
            : base(parser)
        {
            m_T3Hash = new Hashtable();

            m_ConnectProcedure = proc;
            m_IPAddress = IPAddress.Parse(ip);
            m_PortNo = portNo;

            m_T3_Interval = 45;
            m_T5_Interval = 10;
            m_T6_Interval = 5;
            m_T7_Interval = 10;
            m_T8_Interval = 5;
            m_Linktest_Interval = 10;

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

            m_MsgPaser = parser;
        }

        public HsmsDriver(HsmsConnectProcedure proc, string ip, int portNo)
            : this(proc, ip, portNo, new SecsIIMessageParser())
        {            
        }

        void m_Timer_T8_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            m_Timer_T8.Stop();
            //do it job
            m_State = HsmsState.NOT_CONNECTED;
            HsmsDriver_BeginAccept();
        }

        private void T8_Timer_Start()
        {
            System.Diagnostics.Debug.Print("{0}: T8_Timer_Start", m_ConnectProcedure);
            m_Timer_T8.Interval = m_T8_Interval * 1000;
            m_Timer_T8.Start();
        }

        private void T8_Timer_Stop()
        {
            System.Diagnostics.Debug.Print("{0}: T8_Timer_Stop", m_ConnectProcedure);
            m_Timer_T8.Stop();
        }

        private void T6_Timer_Start()
        {
            System.Diagnostics.Debug.Print("{0}: T6_Timer_Start", m_ConnectProcedure);
            m_Timer_T6.Interval = m_T6_Interval * 1000;
            m_Timer_T6.Start();
        }

        private void T6_Timer_Stop() {
            System.Diagnostics.Debug.Print("{0}: T6_Timer_Stop", m_ConnectProcedure);
            m_Timer_T6.Stop();
        }

        void m_Timer_T6_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Diagnostics.Debug.Print("{0}: m_Timer_T6_Elapsed", m_ConnectProcedure);
            m_Timer_T6.Stop();
            T5_Timer_Start();
        }

        void m_Timer_Linktest_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Diagnostics.Debug.Print("{0}: m_Timer_Linktest_Elapsed", m_ConnectProcedure); 
            m_Timer_Linktest.Stop();
            SendLinkTestRequest();
            T6_Timer_Start();
        }

        private void SendLinkTestRequest() {
            System.Diagnostics.Debug.Print("{0}: Linktest.Req", m_ConnectProcedure);
            Send_Request_ControlMessage(SType.LinktestReq);            
        }

        private void SendLinkTestResponse(byte[] data)
        {
            System.Diagnostics.Debug.Print("{0}: Linktest.Resp", m_ConnectProcedure);
            Send_Response_ControlMessage(data);
        }

        private void Send_Response_ControlMessage(byte[] data)
        {
            byte[] sendBuff = new byte[data.Length];
            Array.Copy(data, sendBuff, data.Length);
            sendBuff[9] = (byte)(sendBuff[9] + 1);
            //m_SocketWorker.Send(sendBuff);
            SendBytes(sendBuff);
        }

        //T7 Not Selected timeout
        void m_Timer_T7_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Diagnostics.Debug.Print("{0}: m_Timer_T7_Elapsed", m_ConnectProcedure);
            m_Timer_T7.Stop();
            m_State = HsmsState.NOT_CONNECTED;
            HsmsDriver_BeginAccept();
        }

        void T3_Timer_Start(SecsMessageBase msg)
        {
            System.Diagnostics.Debug.Print("{0}: T3_Timer_Start [TID:{1}]", m_ConnectProcedure, msg.TransactionId);
            T3Timer timer_t3 = new T3Timer(msg);            
            timer_t3.Interval = m_T3_Interval * 1000;
            timer_t3.Elapsed += new System.Timers.ElapsedEventHandler(timer_t3_Elapsed);
            timer_t3.Start();
            m_T3Hash.Add(msg.TransactionId, timer_t3);
        }

        void T3_Timer_Stop(uint transactionId, ref SecsMessageBase priMsg)
        { 
            System.Diagnostics.Debug.Print("{0}: T3_Timer_Stop [TID:{1}]", m_ConnectProcedure, transactionId);
            T3Timer timer_t3 = (T3Timer)m_T3Hash[transactionId];
            priMsg = timer_t3.Message;
            timer_t3.Stop();
            m_T3Hash.Remove(transactionId);
        }

        void timer_t3_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            T3Timer timer_t3 = (T3Timer)sender;
            timer_t3.Stop();

            System.Diagnostics.Debug.Print("{0}: T3_Timer_Elapsed [TID:{1}]", m_ConnectProcedure, timer_t3.TransactionId);
            m_T3Hash.Remove(timer_t3.TransactionId);
            //send S9F9 transaction timedout

            timer_t3.Dispose();
        }

        private Socket m_SocketWorker;
        private Socket m_SocketListener;

        public override void Start() {

            System.Diagnostics.Debug.Print("{0}: Start", m_ConnectProcedure);

            T5_Timer_Stop();
            T6_Timer_Stop();
            T7_Timer_Stop();
            T3_Timer_Stop_All();
            LinkTest_Timer_Stop();               
            
            m_State = HsmsState.NOT_CONNECTED;

            if (m_ConnectProcedure == HsmsConnectProcedure.ACTIVE)
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

        private void T3_Timer_Stop_All() {
            foreach (T3Timer t3_Timer in m_T3Hash.Values)
            {
                t3_Timer.Stop();
            }

            m_T3Hash.Clear();
        }

        public override void Stop()
        {
            System.Diagnostics.Debug.Print("{0}: Stop", m_ConnectProcedure);

            T5_Timer_Stop();
            T6_Timer_Stop();
            T7_Timer_Stop();
            T3_Timer_Stop_All();
            LinkTest_Timer_Stop();

            m_State = HsmsState.NOT_CONNECTED;            

            if (m_SocketWorker != null)
            {
                if (m_SocketWorker.Connected)
                {
                    System.Diagnostics.Debug.Print("{0}: SeparateReq", m_ConnectProcedure);
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
        }

        #region "ACTIVE"

        private void HsmsDriver_BeginConnect()
        {
            if (m_SocketWorker != null && m_SocketWorker.Connected)
            {
                m_SocketWorker.Close();
            }
            m_State = HsmsState.NOT_CONNECTED;
            m_SocketWorker = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_SocketWorker.BeginConnect(new IPEndPoint(m_IPAddress, m_PortNo), HsmsDriver_BeginConnectCallback, null);
        }

        void m_Timer_T5_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Diagnostics.Debug.Print("{0}: m_Timer_T5_Elapsed", m_ConnectProcedure);
            m_Timer_T5.Stop();
            //try to re-connect
            HsmsDriver_BeginConnect();
        }

        private void T5_Timer_Start() {
            System.Diagnostics.Debug.Print("{0}: T5_Timer_Start", m_ConnectProcedure);
            m_Timer_T5.Interval = m_T5_Interval * 1000;
            m_Timer_T5.Start();
        }

        private void T5_Timer_Stop()
        {
            System.Diagnostics.Debug.Print("{0}: T5_Timer_Stop", m_ConnectProcedure);
            m_Timer_T5.Stop();
        }
        
        private void HsmsDriver_BeginConnectCallback(IAsyncResult ar)
        {

            System.Diagnostics.Debug.Print("{0}: HsmsDriver_BeginConnectCallback", m_ConnectProcedure);

            if (m_SocketWorker.Connected)
            {
                m_SocketWorker.EndConnect(ar);
                //change state to NOT_SELECTED
                m_State = HsmsState.NOT_SELECTED;
                //stop T5
                T5_Timer_Stop();
                //wait message
                WaitForMessage_Header_New(m_SocketWorker);
                //send Select.Req
                SendSelectRequest();
                //start T6
                T6_Timer_Start();
            }
            else
            {
                T5_Timer_Start();
            }
        }

        #endregion

        #region "PASSIVE"

        private void HsmsDriver_BeginAccept() {

            System.Diagnostics.Debug.Print("{0}: BeginAccept", m_ConnectProcedure);                       

            if (m_SocketWorker != null && m_SocketWorker.Connected)
            {
                m_SocketWorker.Close();
            }

            if (m_SocketListener == null || !m_SocketListener.IsBound)
            {
                m_SocketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_SocketListener.Bind(new IPEndPoint(IPAddress.Any, m_PortNo)); //will change IsBound to "true"
                m_SocketListener.Listen(1);
            }            

            m_SocketListener.BeginAccept(HsmsDriver_BeginAcceptCallback, null);
        }

        private void HsmsDriver_BeginAcceptCallback(IAsyncResult ar)
        {
            m_SocketWorker = m_SocketListener.EndAccept(ar);
            //hsms state > connect but not select
            m_State = HsmsState.NOT_SELECTED;
            //wait Select.Req   
            WaitForMessage_Header_New(m_SocketWorker);
            //start T7
            T7_Timer_Start();
        }

        private void T7_Timer_Start() {
            System.Diagnostics.Debug.Print("{0}: T7_Timer_Start", m_ConnectProcedure);
            m_Timer_T7.Interval = m_T7_Interval * 1000;
            m_Timer_T7.Start();
        }

        private void T7_Timer_Stop() {
            System.Diagnostics.Debug.Print("{0}: T7_Timer_Stop", m_ConnectProcedure);
            m_Timer_T7.Stop();
        }

        #endregion

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
            System.Diagnostics.Debug.Print("{0}: Select.Req", m_ConnectProcedure);
            Send_Request_ControlMessage(SType.SelectReq);
            //raise event
        }

        private void SendSelectReponse(byte[] data)
        {
            System.Diagnostics.Debug.Print("{0}: Select.Resp", m_ConnectProcedure);
            Send_Response_ControlMessage(data);
        }

        private void SendSeparateRequest()
        {

            System.Diagnostics.Debug.Print("{0}: Separate.Req", m_ConnectProcedure);
            Send_Request_ControlMessage(SType.SelectReq);
            //raise event
        }

        private void Send_Request_ControlMessage(SType sType)
        {
            byte[] msg = CreateControlMessage(sType);
            byte[] tranId = BitConverter.GetBytes(this.GetNextTransactionId());
            Array.Reverse(tranId);
            Array.Copy(tranId, 0, msg, 10, 4);
            //m_SocketWorker.Send(msg);
            SendBytes(msg);
        }

        private void WaitForMessage_Header_New(Socket sck)
        {
            byte[] header = new byte[14];
            WaitForMessage_Header(sck, header, 0);
        }

        private void WaitForMessage_Header(Socket sck, byte[] header, int headerStartIndex)
        {

            //was stop
            if (sck == null && m_State == HsmsState.NOT_CONNECTED)
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

                System.Diagnostics.Debug.Print("{0}: WaitForMessage_Header {1}, {2}",
                    m_ConnectProcedure, header.Length, headerStartIndex);

                sck.BeginReceive(header, headerStartIndex, header.Length - headerStartIndex,
                    SocketFlags.None, out se, WaitForMessage_Header_Callback, holder);

                if (se == SocketError.ConnectionReset)
                {
                    Start();
                }
                
            }
            else
            {
                Start();
            }
        }

        private void WaitForMessage_Header_Callback(IAsyncResult ar)
        {
            SocketStateHolder holder = (SocketStateHolder)ar.AsyncState;

            Socket sck = holder.Socket;
            
            System.Diagnostics.Debug.Print("{0}: WaitForMessage_Header_Callback Connected={1}",
                   m_ConnectProcedure, sck.Connected);


            if (sck.Connected)
            {
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
                        return;
                    }
                }

                if (byteCount == 0) {
                    return;
                }

                holder.RecvStartIndex += byteCount;                
                
                System.Diagnostics.Debug.Print("{0}: WaitForMessage_Header_Callback [ByteCount: {1}]",
                    m_ConnectProcedure, byteCount);

                if (holder.RecvStartIndex == header.Length)
                {
                    byte sType = header[9];
                    uint bodyByteCount = 0;

                    switch (sType)
                    {
                        case 0: //message
                            byte[] lbb = new byte[4];
                            Array.Copy(header, 0, lbb, 0, 4);
                            Array.Reverse(lbb);
                            bodyByteCount = BitConverter.ToUInt32(lbb, 0) - 10; //exclude header
                            bool needBody = (bodyByteCount > 0); //header
                            if (needBody)
                            {
                                byte[] body = new byte[bodyByteCount];
                                WaitForMessage_Body(sck, header, body, 0);
                                return;
                            }
                            else
                            {
                                //process secs message
                                ProcessInCommingMessage(header);
                            }
                            break;
                        case 1: //Select.Req   
                            m_State = HsmsState.SELECTED;
                            //stop T7
                            T7_Timer_Stop();
                            //Select.Req relate with T5
                            SendSelectReponse(header);
                            //raise event 
                            RaiseEventOnSelected();
                            break;
                        case 2: //Select.Resp
                            m_State = HsmsState.SELECTED;
                            //T6 timer
                            T6_Timer_Stop();
                            //start link test timer
                            LinkTest_Timer_Start();
                            //raise event
                            RaiseEventOnSelected();
                            break;
                        case 5: //Linktest.Req
                            SendLinkTestResponse(header);
                            break;
                        case 6: //Linktest.Resp
                            //T6 timer stop
                            T6_Timer_Stop();
                            //l
                            LinkTest_Timer_Start();
                            break;
                        case 9: //Separate.Req
                            m_State = HsmsState.NOT_CONNECTED;
                            break;
                        default:
                            //unknow
                            break;
                    }

                    WaitForMessage_Header_New(sck);

                    //** be careful when added more code below this section
                    //** because at " case 0: //message" have the "return;" statement
                    //** that might cause somethings go wrong
                }
                else
                {
                    WaitForMessage_Header(sck, header, holder.RecvStartIndex);
                }
            }
            //let the timer does its job
            //else
            //{
            //    Start();
            //}

            holder.Socket = null;
            holder.Header = null;
            holder.Body = null;
            holder = null;
        }

        private void LinkTest_Timer_Start()
        {
            m_Timer_Linktest.Interval = m_Linktest_Interval * 1000;
            m_Timer_Linktest.Start();
        }

        private void LinkTest_Timer_Stop()
        {
            m_Timer_Linktest.Stop();
        }

        private void WaitForMessage_Body(Socket sck, byte[] header, byte[] body, int bodyStartIndex)
        {
            if (sck.Connected)
            {
                System.Diagnostics.Debug.Print("{0}: WaitForMessage_Body {1}, {2}, {3}",
                            m_ConnectProcedure, header.Length, body.Length, bodyStartIndex);

                SocketError se;
                SocketStateHolder holder = new SocketStateHolder();
                holder.Socket = sck;
                holder.Header = header;
                holder.Body = body;
                holder.RecvStartIndex = bodyStartIndex;

                m_SocketWorker.BeginReceive(body, bodyStartIndex, body.Length - bodyStartIndex, SocketFlags.None,
                   out se, WaitForMessage_Body_Callback, holder);

                if (se == SocketError.ConnectionReset)
                {
                    //Start();
                    //let T8 work
                }
            }
            //let the timer does its job
            //else
            //{
            //    Start();
            //}
        }

        private void WaitForMessage_Body_Callback(IAsyncResult ar)
        {
            if (m_SocketWorker.Connected)
            {
                SocketStateHolder holder = (SocketStateHolder)ar.AsyncState;
                byte[] header = holder.Header;
                byte[] body = holder.Body;
                
                int rcvByteCount = m_SocketWorker.EndReceive(ar);

                System.Diagnostics.Debug.Print("{0}: WaitForMessage_Body_Callback [ByteCount: {1}]",
                    m_ConnectProcedure, rcvByteCount); 

                holder.RecvStartIndex += rcvByteCount;

                if (holder.RecvStartIndex == body.Length)
                {
                    byte[] data = new byte[header.Length + body.Length];
                    Array.Copy(header, 0, data, 0, header.Length);
                    Array.Copy(body, 0, data, header.Length, body.Length);
                    //process secs message
                    ProcessInCommingMessage(data);
                    //continue to next message
                    WaitForMessage_Header_New(holder.Socket);
                }
                else
                {
                    WaitForMessage_Body(holder.Socket, header, body, holder.RecvStartIndex);
                }

            }
        }

        /// <summary>
        /// send without checking anything !!!!
        /// </summary>
        /// <param name="data"></param>
        protected override void SendBytes(byte[] data)
        {
            if (m_SocketWorker.Connected)
            {
                m_SocketWorker.BeginSend(data, 0, data.Length, SocketFlags.None, SendBytesCallback, data);
            }
            else
            {
                Start();
            }
        }       

        private void SendBytesCallback(IAsyncResult ar)
        {
            if (m_SocketWorker.Connected)
            {
                try
                {
                    byte[] data = (byte[])ar.AsyncState;
                    int sentByteCount = m_SocketWorker.EndSend(ar);

                    if (data.Length == sentByteCount)
                    {
                        //raise event sent

                        //if primary message
                    }
                }
                catch
                {
                    Start();
                }

            }
        }

        private void ProcessInCommingMessage(byte[] data)
        {          
            SecsMessageBase msg = m_MsgPaser.ToSecsMessage(data);

            if ((msg.Function & 0x01) == 1) //primary message
            {
                RaiseEventReceivedPrimaryMessage(msg);
            }
            else
            {
                SecsMessageBase priMsg = null;
                if (m_T3Hash.ContainsKey(msg.TransactionId))
                {
                    T3_Timer_Stop(msg.TransactionId, ref priMsg);
                }
                RaiseEventReceivedSecondaryMessage(priMsg, msg);
            }
        }

        private void RaiseEventOnSelected()
        {
            m_SyncContext.Post(PostEventOnSelected, null);            
        }

        private void PostEventOnSelected(object state)
        {
            if (OnSelected != null)
            {
                OnSelected(this, EventArgs.Empty);
            }
        }

        protected override void OnSendingPrimaryMessage(SecsMessageBase msg)
        {
            T3_Timer_Start(msg);
        }
    
    }
}
