using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Timers;
using System.IO;
using System.Collections;

namespace XtraLibrary.SecsGem.Drivers
{
    public class SecsIDriver
        :SecsDriverBase
    {

        #region "Properties"

        public SerialPort Port
        {
            get
            {
                return m_Port;
            }
        }

        public double T1_Interval
        {
            get { return m_T1_Interval; }
            set {
                if (value < 0.1d || value > 10d)
                {
                    throw new Exception("Value is out of range 0.1 - 10 secs");
                }
                m_T1_Interval = value; 
            }
        }

        public double T2_Interval
        {
            get { return m_T2_Interval; }
            set {
                if (value < 0.2d || value > 25d)
                {
                    throw new Exception("Value is out of range 0.2 - 25 secs");
                }
                m_T2_Interval = value; 
            }
        }

        public double T3_Interval
        {
            get { return m_T3_Interval; }
            set {
                if (value < 1d || value > 120d)
                {
                    throw new Exception("Value is out of range 1 - 120 secs");
                }
                m_T3_Interval = value; 
            }
        }

        //T4 no use !!!!

        /// <summary>
        /// The maximum number of send retries allowed
        /// </summary>
        public int RTY
        {
            get
            {
                return m_RTY;
            }
            set
            {
                if (value < 0d || value > 31d)
                {
                    throw new Exception("Value is out of range 0 - 31");
                }
                m_RTY = value;
            }
        }

        /// <summary>
        /// Now not use
        /// </summary>
        public SecsIContentionType Contention
        {
            get
            {
                return m_Contention;
            }
            set
            {
                m_Contention = value;
            }
        }

        #endregion

        #region "Variables"

        private CommuState m_State;
        private SerialPort m_Port;

        private SecsIMessageParser m_MsgParser;

        private int m_RecvBuffIndex;
        private byte[] m_RecvBuff;

        private byte[] m_ENQ;
        private byte[] m_EOT;
        private byte[] m_ACK;
        private byte[] m_NAK;

        //SECS-I timeout
        private Timer m_T1_Timer = new Timer();
        private Timer m_T2_Timer = new Timer();

        private double m_T1_Interval;
        private double m_T2_Interval;
        private double m_T3_Interval;

        private int m_RTY;

        private SecsIContentionType m_Contention;
        
        List<byte[]> m_MultiBlockList;

        private Queue<byte[]> m_QueueSendingBlock;

        private Hashtable m_T3Hash;

        #endregion

        private enum HandshakeCodes :byte
        { 
            ENQ = 5,
            EOT = 4,
            ACK = 6,
            NAK = 21
        }

        private enum CommuState
        { 
            Idle = 0,
            Send_Request = 1,
            Sending = 2,
            Receiving = 3
        }

        #region "Constructor"

        public SecsIDriver(SecsIMessageParser parser)
            :base(parser)
        {
            m_QueueSendingBlock = new Queue<byte[]>();

            m_RecvBuff = new byte[1024]; //actually use only 258 bytes

            m_MsgParser = new SecsIMessageParser();

            m_Port = new SerialPort();
            m_Port.DataReceived += new SerialDataReceivedEventHandler(m_Port_DataReceived);
            
            m_ENQ = new byte[] { 5 };
            m_EOT = new byte[] { 4 };
            m_ACK = new byte[] { 6 };
            m_NAK = new byte[] { 7 };

            m_T1_Timer = new Timer();
            m_T1_Timer.Elapsed += new ElapsedEventHandler(T1_Elapsed);
            m_T2_Timer = new Timer();
            m_T2_Timer.Elapsed += new ElapsedEventHandler(T2_Elapsed);

            m_T1_Interval = 0.5d;
            m_T2_Interval = 10d;
            m_T3_Interval = 45;

            m_RTY = 3;
            
            m_State = CommuState.Idle;

            m_Contention = SecsIContentionType.Slave;

            m_MultiBlockList = new List<byte[]>();

            m_T3Hash = new Hashtable();
        }

        public SecsIDriver()
            :this(new SecsIMessageParser())
        {
        }

        #endregion

        #region "Timers"

        private void Start_T1_Timer()
        {
            m_T1_Timer.Interval = m_T1_Interval * 1000;
            m_T1_Timer.Start();
        }

        private void Stop_T1_Timer()
        {
            m_T1_Timer.Stop();
        }

        private void Start_T2_Timer()
        {
            m_T2_Timer.Interval = m_T2_Interval * 1000;
            m_T2_Timer.Start();
        }

        private void Stop_T2_Timer()
        {
            m_T2_Timer.Stop();
        }

        void Start_T3_Timer(SecsMessageBase msg)
        {
            System.Diagnostics.Debug.Print("Start_T3_Timer [TID:{0}]", msg.TransactionId);

            T3Timer timer_t3 = new T3Timer(msg);
            timer_t3.Interval = m_T3_Interval * 1000;
            timer_t3.Elapsed += new System.Timers.ElapsedEventHandler(T3_Elapsed);
            timer_t3.Start();
            m_T3Hash.Add(msg.TransactionId, timer_t3);
        }

        void Stop_T3_Timer(uint transactionId, ref SecsMessageBase priMsg)
        {
            System.Diagnostics.Debug.Print("Stop_T3_Timer [TID:{0}]", transactionId);

            T3Timer timer_t3 = (T3Timer)m_T3Hash[transactionId];
            priMsg = timer_t3.Message;
            timer_t3.Stop();
            m_T3Hash.Remove(transactionId);
        }

        void T3_Elapsed(object sender, ElapsedEventArgs e)
        {
            T3Timer timer_t3 = (T3Timer)sender;
            timer_t3.Stop();            
            m_T3Hash.Remove(timer_t3.TransactionId);

            System.Diagnostics.Debug.Print("T3_Elapsed [TID:{0}]", timer_t3.TransactionId);                
            timer_t3.Dispose();
        }

        /// <summary>
        /// T2 TIMEOUT
        ///     1. NO EOT AFTER ENQ
        ///     2. NO ACK/NAK AFTER MESSAGE
        ///     3. NO MESSAGE AFTER EOT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void T2_Elapsed(object sender, ElapsedEventArgs e)
        {

        }

        void T1_Elapsed(object sender, ElapsedEventArgs e)
        {
            m_T1_Timer.Stop();
            m_State = CommuState.Idle;
            SendHandshakeCode(HandshakeCodes.NAK);
        }

        #endregion

        #region "Receiving"

        void m_Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (this)
            {
                byte[] buff = new byte[m_Port.BytesToRead];
                int length = m_Port.Read(buff, 0, buff.Length);
                
                System.Diagnostics.Debug.Print("DataReceived {0} bytes , State {1}", length, m_State);
                System.Diagnostics.Debug.Print("Hex : " + GetHexString(buff));

                if (length == 0)
                {
                    return;
                }

                switch(m_State)
                {
                    case CommuState.Idle :
                        if (length == 1)
                        {
                            System.Diagnostics.Debug.Print("RecvControlChar := "+ (HandshakeCodes)buff[0]);
                            if ((HandshakeCodes)buff[0] == HandshakeCodes.ENQ)
                            {
                                m_State = CommuState.Receiving;
                                SendHandshakeCode(HandshakeCodes.EOT);
                                //3. NO MESSAGE AFTER EOT
                                Start_T1_Timer();
                            }
                        }
                        break;
                    case CommuState.Receiving:
                        Stop_T2_Timer();
                        OnDataArrival(buff, length);
                        
                        break;
                    case CommuState.Send_Request :
                        System.Diagnostics.Debug.Print("RecvControlChar := " + (HandshakeCodes)buff[0]);
                        if ((HandshakeCodes)buff[0] == HandshakeCodes.EOT)
                        {
                            Stop_T2_Timer();
                            m_State = CommuState.Sending;
                            StartSend();
                        }
                        break;
                    case CommuState.Sending:
                        System.Diagnostics.Debug.Print("RecvControlChar := " + (HandshakeCodes)buff[0]);
                        if ((HandshakeCodes)buff[0] == HandshakeCodes.ACK)
                        {
                            Stop_T2_Timer();
                            m_State = CommuState.Idle;
                        }
                        else if ((HandshakeCodes)buff[0] == HandshakeCodes.NAK)
                        {
                            Stop_T2_Timer();
                            m_State = CommuState.Idle; //message failed
                        }
                        break;
                }
            }

            if (m_State ==  CommuState.Idle && m_QueueSendingBlock.Count > 0)
            {
                RequestToSend();
            }
        }
        
        public void OnDataArrival(byte[] buffer, int length)
        {            
            Array.Copy(buffer, 0, m_RecvBuff, m_RecvBuffIndex, length);
            m_RecvBuffIndex += length;

            System.Diagnostics.Debug.Print("RecvBuffIndex := " + m_RecvBuffIndex.ToString());

            if (m_RecvBuffIndex > 0)
            {
                //LTH[0] << length of data 1 + CHECKSUM 2  
                byte lth = m_RecvBuff[0];

                Stop_T1_Timer();

                //wait until end of block
                if (lth + 3 == m_RecvBuffIndex)
                {
                    ushort checksumCal = 0;
                    for (int i = 1; i < m_RecvBuffIndex - 2; i++)
                    {
                        checksumCal += m_RecvBuff[i];
                    }

                    ushort checksumRecv = BitConverter.ToUInt16(
                        new byte[] { 
                        m_RecvBuff[m_RecvBuffIndex - 1], 
                        m_RecvBuff[m_RecvBuffIndex - 2]
                    }, 0);

                    if (checksumCal == checksumRecv)
                    {
                        //change State to idle
                        m_State = CommuState.Idle;
                        //if CHECK SUM is correct
                        SendHandshakeCode(HandshakeCodes.ACK);

                        byte[] header = new byte[10];

                        Array.Copy(m_RecvBuff, 1, header, 0, header.Length);

                        System.Diagnostics.Debug.Print("HEAD {0} bytes ", header.Length);

                        byte[] transactionIdBytes = new byte[4];
                        Array.Copy(header, 6, transactionIdBytes, 0, 4);
                        Array.Reverse(transactionIdBytes);

                        //transaction Id alway same in Multi-Block message
                        uint tid = BitConverter.ToUInt32(transactionIdBytes, 0);
                        System.Diagnostics.Debug.Print("Transaction ID {0} bytes ", tid);

                        int bodyLength = lth - 10;

                        if (bodyLength > 0)
                        {
                            byte[] body = new byte[bodyLength];
                            Array.Copy(m_RecvBuff, 11, body, 0, bodyLength);

                            System.Diagnostics.Debug.Print("BODY {0} bytes ", body.Length);

                            m_MultiBlockList.Add(body);
                        }

                        if ((header[4] & 0x80) == 0x80) //last block
                        {
                            //build up all message 
                            int totalBytes = 10; //header

                            foreach (byte[] bytes in m_MultiBlockList)
                            {
                                totalBytes += bytes.Length;
                            }

                            byte[] data = new byte[totalBytes];
                            int index = 0;

                            Array.Copy(header, 0, data, 0, header.Length);
                            index += header.Length;

                            foreach (byte[] bytes in m_MultiBlockList)
                            {
                                Array.Copy(bytes, 0, data, index, bytes.Length);
                                index += bytes.Length;
                            }

                            m_MultiBlockList.Clear();

                            ProcessInCommingMessage(data);
                        }
                    }
                    else
                    {
                        //send NAK for check sum error
                        SendHandshakeCode(HandshakeCodes.NAK);
                    }

                    //clear buffer
                    Array.Clear(m_RecvBuff, 0, m_RecvBuffIndex);
                    m_RecvBuffIndex = 0;
                }
                else
                {
                    Start_T1_Timer();
                }
            } //if (m_RecvBuffIndex > 0)
        }
        
        public void ProcessInCommingMessage(byte[] data)
        {
            System.Diagnostics.Debug.Print("ProcessInCommingMessage {0} bytes ", data.Length);
            System.Diagnostics.Debug.Print("Hex : " + GetHexString(data));

            SecsMessageBase msg = m_MsgParser.ToSecsMessage(data);

            if ((msg.Function & 0x01) == 1) //primary message
            {
                RaiseEventReceivedPrimaryMessage(msg);
            }
            else
            {
                SecsMessageBase priMsg = null;
                if (m_T3Hash.ContainsKey(msg.TransactionId))
                {
                    Stop_T3_Timer(msg.TransactionId, ref priMsg);
                    System.Diagnostics.Debug.Print("RaiseEventReceivedSecondaryMessage :=" + msg.TransactionId); 
                }
                RaiseEventReceivedSecondaryMessage(priMsg, msg);
            }
        }

        #endregion

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

        #region "Sending"

        private void StartSend()
        {
            lock (this)
            {                
                byte[] data = m_QueueSendingBlock.Dequeue();

                System.Diagnostics.Debug.Print("StartSend {0}", data.Length);
                System.Diagnostics.Debug.Print("Hex :=" + GetHexString(data));

                m_Port.Write(data, 0, 1);
                m_Port.Write(data, 1, data.Length - 3);
                m_Port.Write(data, data.Length - 2, 2);

                //2. NO ACK/NAK AFTER MESSAGE
                Start_T2_Timer();
            }
        }

        #endregion

        public override void Start()
        {
            System.Diagnostics.Debug.Print("Start");
            m_Port.Open();
            System.Diagnostics.Debug.Print("PortIsOpen =" + m_Port.IsOpen);
        }

        public override void Stop()
        {
            System.Diagnostics.Debug.Print("Stop");
            m_Port.Close();
        }
        
        //reserv for secs message only
        protected override void SendBytes(byte[] data)
        {
            System.Diagnostics.Debug.Print("SendBytes [" + GetHexString(data) + "]");

            //append message to queue
            AddSecsMessageToBlockQueueing(data);
            
            if (m_State == CommuState.Idle)
            {
                RequestToSend();
            }
           
        }

        private void RequestToSend()
        {
            m_State = CommuState.Send_Request;
            SendHandshakeCode(HandshakeCodes.ENQ);
            //1. NO EOT AFTER BID (ENQ)
            Start_T2_Timer();
        }

        private void AddSecsMessageToBlockQueueing(byte[] secsMessageBytes)
        {
            lock (this)
            {

                int index = 0;
                int byteToRead = 0;
                
                byte[] header = new byte[10];
                Array.Copy(secsMessageBytes, 0, header, 0, header.Length);

                //The R-bit is set to 0 for messages to the equipment and set to 1 for messages to the host
                header[0] = (byte)(header[0] & 0x7F);
                
                ushort blockNo = 0;
                bool isLastBlock;
                byte[] msgBlock;
                                
                int remainingBodyByteCount = secsMessageBytes.Length - 10;

                //245 = Maximum byte of Body in each time
                int sendTimes = (remainingBodyByteCount == 0 ? 1 : (int)Math.Ceiling((double)(remainingBodyByteCount) / 245d));

                System.Diagnostics.Debug.Print("AddSecsMessageToBlockQueueing {0} block(s)", sendTimes);

                index = 10; //skip header

                for (int i = 0; i < sendTimes; i++)
                {
                    blockNo += 1;

                    if (remainingBodyByteCount >= 245)
                    {
                        byteToRead = 245; 
                    }
                    else
                    {
                        byteToRead = remainingBodyByteCount; //exclude header
                    }

                    msgBlock = new byte[byteToRead + 13];// LTH + header + body + sum
                    //LTH
                    msgBlock[0] = (byte)(msgBlock.Length - 3);                    
                    //header 
                    Array.Copy(header, 0, msgBlock, 1, header.Length);

                    if (byteToRead > 0)
                    {
                        // destination index 11 > LTH + header 
                        Array.Copy(secsMessageBytes, index, msgBlock, 11, byteToRead);
                        index += byteToRead;
                        remainingBodyByteCount -= byteToRead;
                    }

                    //modify BLOCK NO section in header
                    isLastBlock = (remainingBodyByteCount == 0);
                    byte[] blockNoBytes = BitConverter.GetBytes(blockNo);
                    Array.Reverse(blockNoBytes);
                    //BLOCK NO
                    msgBlock[5] = (byte)(isLastBlock ? 
                        (blockNoBytes[0] | 0x80): 
                        (blockNoBytes[0] & 0x7F));
                    msgBlock[6] = blockNoBytes[1];
            
                    //SUM
                    ushort checksum = 0;

                    for (int j = 1; j < msgBlock.Length - 2; j++) //-2 exclude check sum
                    {
                        checksum += msgBlock[j];
                    }

                    byte[] sumBytes = BitConverter.GetBytes(checksum);
                    Array.Reverse(sumBytes);
                    //11 = LTH + HEADER length
                    Array.Copy(sumBytes, 0, msgBlock, msgBlock.Length - 2, 2);
                    m_QueueSendingBlock.Enqueue(msgBlock);
                }
            }
        }

        private void SendHandshakeCode(HandshakeCodes chr)
        {

            System.Diagnostics.Debug.Print("SendControlChar := {0}", chr);

            byte[] b = null;
            switch (chr)
            { 
                case HandshakeCodes.ACK:
                    b = m_ACK;
                    break;
                case HandshakeCodes.ENQ:
                    b = m_ENQ;
                    break;
                case HandshakeCodes.EOT:
                    b = m_EOT;
                    break;
                case HandshakeCodes.NAK:
                    b = m_NAK;
                    break;
            }

            if (b != null)
            { 
                m_Port.Write(b, 0, b.Length);
            }
            
        }

        protected override void OnSendingPrimaryMessage(SecsMessageBase msg)
        {
            Start_T3_Timer(msg);
        }

    }
}
