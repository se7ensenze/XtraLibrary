using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsItemBinary :SecsItem<byte[]>
    {
        public SecsItemBinary(string name)
            : base(FormatCode.Binary, name, null)
        { 
        }

        public SecsItemBinary(string name, params byte[] val)
            : base(FormatCode.Binary, name, val)
        {
        }

        protected override void ReadValue(System.IO.MemoryStream reader, uint byteToRead)
        {
            byte[] buff = new byte[byteToRead];
            reader.Read(buff, 0, buff.Length);
            this.Value = buff;
        }

        protected override byte[] GetDataBytes()
        {
            return this.Value;
        }
    }
}
