using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsItemBoolean :SecsItem<Boolean[]>
    {

        public SecsItemBoolean(string name) 
            : base (FormatCode.Boolean, name, null) { 
        }

        public SecsItemBoolean(string name, params bool[] val)
            : base(FormatCode.Boolean, name, val)
        {
        }       

        protected override void ReadValue(System.IO.MemoryStream reader, uint byteToRead)
        {
            byte[] buff = new byte[byteToRead];
            reader.Read(buff, 0, buff.Length);

            bool[] vals = new bool[byteToRead];
            for (int i = 0; i < buff.Length; i++ )
            {
                vals[i] = BitConverter.ToBoolean(buff, i);
            }
         
            this.Value = vals;                    // 160708 \783 Return Null
        
        }

        protected override byte[] GetDataBytes()
        {
            bool[] vals = this.Value;

            if (vals == null || vals.Length == 0)
            {
                return null;
            }

            byte[] buff = new byte[Value.Length];
            for (int i = 0; i < buff.Length; i++)
            {
                buff[i] = BitConverter.GetBytes(vals[i])[0];
            }
            return buff;
        }
    }
}
