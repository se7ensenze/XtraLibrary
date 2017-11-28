using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace XtraLibrary.SecsGem
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class SVIDAttribute
        :Attribute
    {
        private object m_ID;
        public object ID
        {
            get {
                return m_ID;
            }
        }

        private FormatCode m_Format;
        public FormatCode Format
        {
            get {
                return m_Format;
            }
        }

        private int m_Size;
        public int Size
        {
            get {
                return m_Size;
            }
        }

        public const int SIZE_N = -1;

        public SVIDAttribute(FormatCode fc, object svId)
            : this(fc, svId, 1)
        {            
        }

        public SVIDAttribute(FormatCode fc, object svId, int size)
        {
            m_Format = fc;
            m_Size = size;

            string strSvid = svId.ToString();
            
            switch (fc)
            { 
                case FormatCode.U1:
                    sbyte u1 = 0;
                    if (sbyte.TryParse(strSvid, out u1))
                    {
                        m_ID = u1;
                    }
                    break;
                case FormatCode.U2:
                    ushort u2 = 0;
                    if (ushort.TryParse(strSvid, out u2))
                    {
                        m_ID = u2;
                    }
                    break;
                case FormatCode.U4:
                    UInt32 u4 = 0;
                    if (UInt32.TryParse(strSvid, out u4))
                    {
                        m_ID = u4;
                    }
                    break;
                case FormatCode.U8:
                    UInt64 u8 = 0;
                    if (UInt64.TryParse(strSvid, out u8))
                    {
                        m_ID = u8;
                    }
                    break;
                case FormatCode.I1:
                    byte i1 = 0;
                    if (byte.TryParse(strSvid, out i1))
                    {
                        m_ID = i1;
                    }
                    break;
                case FormatCode.I2:
                    short i2 = 0;
                    if (short.TryParse(strSvid, out i2))
                    {
                        m_ID = i2;
                    }
                    break;
                case FormatCode.I4:
                    Int32 i4 = 0;
                    if (Int32.TryParse(strSvid, out i4))
                    {
                        m_ID = i4;
                    }
                    break;
                case FormatCode.I8:
                    Int64 i8 = 0;
                    if (Int64.TryParse(strSvid, out i8))
                    {
                        m_ID = i8;
                    }
                    break;
                case FormatCode.ASCII:
                    m_ID = strSvid;
                    break;
                default :
                    //throw error
                    throw new Exception("Not supported type " + fc.ToString());
            }            
        }
    }   

}
