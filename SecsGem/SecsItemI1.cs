using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsItemI1:SecsItem<sbyte[]>
    {

        public SecsItemI1(string name)
            : base(FormatCode.I1, name, null)
        { 
        }

        public SecsItemI1(string name, params sbyte[] val)
            : base(FormatCode.I1, name, val)
        {
        }

        protected override void ReadValue(System.IO.MemoryStream reader, uint byteToRead)
        {
            byte[] buff = new byte[byteToRead];
            reader.Read(buff, 0, buff.Length);
            unchecked
	        {
                this.Value = new sbyte[buff.Length];
                for (int i = 0; i < buff.Length; i++)
                {
                    this.Value[i] = (sbyte)buff[i];
                }
	        }            
        }

        protected override byte[] GetDataBytes()
        {
            sbyte[] vals = this.Value;

            if (vals == null || vals.Length == 0)
            {
                //zero length item
                return null;
            }

            byte[] buff = new byte[vals.Length];
            unchecked
            {
                for (int i = 0; i < buff.Length; i++)
                {
                    buff[i] = (byte)vals[i];
                }
            }
            return buff;
        }
    }
}
