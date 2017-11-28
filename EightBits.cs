using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary
{

    public class EightBits
    {
        private byte[] m_Bits;

        public EightBits() {
            m_Bits = new byte[8];
        }

        public EightBits(byte b) 
            :this()
        {
            byte val = b;
            for (int i = 0; i < m_Bits.Length; i++)
            {
                m_Bits[i] = (byte)(val & 0x01);
                val = (byte)(val >> 1);
            }
        }

        public byte ToByte()
        {
            byte b = 0;
            for (int i = m_Bits.Length - 1; i >= 0; i--)
            {
                b = (byte)(b << 1);
                b += m_Bits[i];
            }
            return b;
        }

        public byte Bit1
        {
            get
            {
                return m_Bits[0];
            }
            set
            {
                if (value > 1)
                {
                    throw new OverflowException("value should be 0 or 1");
                }
                m_Bits[0] = value;
            }
        }

        public byte Bit2
        {
            get
            {
                return m_Bits[1];
            }
            set
            {
                if (value > 1)
                {
                    throw new OverflowException("value should be 0 or 1");
                }
                m_Bits[1] = value;
            }
        }

        public byte Bit3
        {
            get
            {
                return m_Bits[2];
            }
            set
            {
                if (value > 1)
                {
                    throw new OverflowException("value should be 0 or 1");
                }
                m_Bits[2] = value;
            }
        }

        public byte Bit4
        {
            get
            {
                return m_Bits[3];
            }
            set
            {
                if (value > 1)
                {
                    throw new OverflowException("value should be 0 or 1");
                }
                m_Bits[3] = value;
            }
        }

        public byte Bit5
        {
            get
            {
                return m_Bits[4];
            }
            set
            {
                if (value > 1)
                {
                    throw new OverflowException("value should be 0 or 1");
                }
                m_Bits[4] = value;
            }
        }

        public byte Bit6
        {
            get
            {
                return m_Bits[5];
            }
            set
            {
                if (value > 1)
                {
                    throw new OverflowException("value should be 0 or 1");
                }
                m_Bits[5] = value;
            }
        }

        public byte Bit7
        {
            get
            {
                return m_Bits[6];
            }
            set
            {
                if (value > 1)
                {
                    throw new OverflowException("value should be 0 or 1");
                }
                m_Bits[6] = value;
            }
        }

        public byte Bit8
        {
            get
            {
                return m_Bits[7];
            }
            set
            {
                if (value > 1)
                {
                    throw new OverflowException("value should be 0 or 1");
                }
                m_Bits[7] = value;
            }
        }

        public override string ToString()
        {
            string ret = null;
            for (int i = m_Bits.Length - 1; i >= 0; i--)
            {
                ret += m_Bits[i].ToString();
            }
            return ret;
        }

        public static explicit operator byte (EightBits eb)
        {
            return eb.ToByte();
        }

        public static explicit operator EightBits(byte b)
        {
            return new EightBits(b);
        }

    }
}
