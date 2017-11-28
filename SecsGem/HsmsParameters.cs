using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class HsmsParameters
    {
        private int m_T3_Interval;
        private int m_T5_Interval;
        private int m_T6_Interval;
        private int m_T7_Interval;
        private int m_T8_Interval;
        private int m_Linktest_Interval;
        private bool m_LinktestEnabled;

        private string m_IPAddress;
        private int m_PortNo;

        private HsmsConnectProcedure m_Mode;

        public HsmsParameters()
        {
            m_Mode = HsmsConnectProcedure.ACTIVE;
            m_IPAddress = "127.0.0.1";
            m_PortNo = 5040;

            m_T3_Interval = 45;
            m_T5_Interval = 10;
            m_T6_Interval = 5;
            m_T7_Interval = 10;
            m_T8_Interval = 5;
            m_Linktest_Interval = 10;
            m_LinktestEnabled = true;
        }

        public int T3_Interval
        {
            get
            {
                return m_T3_Interval;
            }
            set
            {
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

        public bool LinktestEnabled
        {
            get
            {
                return m_LinktestEnabled;
            }

            set
            {
                m_LinktestEnabled = value;
            }
        }

        public HsmsConnectProcedure Mode
        {
            get
            {
                return m_Mode;
            }
            set
            {
                m_Mode = value;
            }
        }

        public string IPAddress
        {
            get
            {
                return m_IPAddress;
            }
            set
            {
                m_IPAddress = value;
            }

        }

        /// <summary>
        /// Remote or Local Port 
        /// </summary>
        public int PortNo
        {
            get
            {
                return m_PortNo;
            }
            set
            {
                m_PortNo = value;
            }
        }
    }
}
