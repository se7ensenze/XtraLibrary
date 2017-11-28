using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsIParameters
    {
        private string m_PortName;
        private int m_BaudRate;
        private int m_RTY;
        private double m_T1_Interval;
        private double m_T2_Interval;
        private double m_T3_Interval;
        //private double m_T4_Interval;

        public SecsIParameters()
        {
            m_PortName = "COM1";
            m_BaudRate = 9600;
            m_RTY = 3;
            m_T1_Interval = 0.5d;
            m_T2_Interval = 11;
            m_T3_Interval = 45;
            //m_T4_Interval = 45;
        }

        public double T1_Interval
        {
            get { return m_T1_Interval; }
            set
            {
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
            set
            {
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
            set
            {
                if (value < 1d || value > 120d)
                {
                    throw new Exception("Value is out of range 1 - 120 secs");
                }
                m_T3_Interval = value;
            }
        }

        public string PortName
        {
            get
            {
                return m_PortName;
            }
            set
            {
                m_PortName = value;
            }
        }

        public int BaudRate
        {
            get
            {
                return m_BaudRate;
            }
            set
            {
                m_BaudRate = value;
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

    }
}
