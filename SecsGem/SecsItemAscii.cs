using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsItemAscii :SecsItem<string>
    {

        public SecsItemAscii(string name)
            : base(FormatCode.ASCII, name, null) 
        {             
        }

        public SecsItemAscii(string name, string val)
            : base(FormatCode.ASCII, name, val)
        {
        } 

        protected override void ReadValue(System.IO.MemoryStream reader, uint byteToRead)
        {
            byte[] buff = new byte[byteToRead];
            reader.Read(buff, 0, buff.Length);
            this.Value = Encoding.ASCII.GetString(buff, 0, buff.Length);
        }    

        protected override byte[] GetDataBytes()
        {
            if (this.Value == null)
            {
                return new byte[0];
            }
            else
            {
                return Encoding.ASCII.GetBytes(this.Value);
            }            
        }
    }
}
