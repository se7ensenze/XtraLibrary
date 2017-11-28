using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsItemMultiByteChar : SecsItem<string>
    {

        public SecsItemMultiByteChar(string name)
            : base(FormatCode.MC, name)
        { 
        }

        public SecsItemMultiByteChar(string name, string val)
            : base(FormatCode.MC, name, val)
        {
        } 

        public enum EncodingType :ushort
        {
            None = 0,
            ISO_10646_UCS_2 = 1,
            UTF8 = 2,
            ISO_646_1991 = 3,
            ISO_8859_1 = 4,
            ISO_8859_11 = 5,
            TIS_620 = 6,
            IS_13194 = 7,
            ShiftJIS = 8,
            Japanese_EUC_JP = 9,
            Korean_EUC_KR = 10,
            SimplifiedChinese_GB = 11,
            SimplifiedChinese_EUC_CN = 12,
            TraditionalChinese_Big5 = 13,
            TraditionalChinese_EUC_TW = 14
        }

        private EncodingType m_TextEncodType;
        public EncodingType TextEncodType
        {
            get { return m_TextEncodType; }
            set
            {
                m_TextEncodType = value;
            }
        }			

        protected override void ReadValue(System.IO.MemoryStream reader, uint byteToRead)
        {
            byte[] encoding = new byte[2]; //2 byte after encoding
            reader.Read(encoding, 0, encoding.Length);

            try
            {
                m_TextEncodType = (EncodingType)BitConverter.ToUInt16(encoding, 0);
            }
            catch
            { 
                //unknow encoding
            }

            byte[] data = new byte[byteToRead - encoding.Length];
            reader.Read(data, 0, data.Length);
        }

        protected override byte[] GetDataBytes()
        {
            string vals = this.Value;

            if (vals == null || vals.Length == 0)
            {
                //zero length item
                return null;
            }

            byte[] encoding = new byte[2];
            byte[] textBytes = null;

            byte[] data = new byte[encoding.Length + textBytes.Length];
            Array.Copy(encoding, 0, data, 0, encoding.Length);
            Array.Copy(textBytes, 0, data, encoding.Length, textBytes.Length);

            return data;
        }
    }
}
