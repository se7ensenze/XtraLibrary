using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsItemU1 :SecsItem<byte[]>
    {

        public SecsItemU1(string name)
            : base(FormatCode.U1, name, null)
        { 
        }

        public SecsItemU1(string name, params byte[] val)
            : base(FormatCode.U1, name, val)
        {
        }

        protected override byte[] GetDataBytes()
        {

            byte[] vals = this.Value;

            if (vals == null || vals.Length == 0)
            {
                //zero length item
                return null;
            }

            return vals;
        }

        protected override void ReadValue(System.IO.MemoryStream reader, uint byteToRead)
        {
            byte[] data = new byte[byteToRead];
            reader.Read(data, 0, data.Length);
            this.Value = data;
        }
    }
}
