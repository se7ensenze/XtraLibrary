using System;
using System.Collections.Generic;
using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsItemU2 :SecsItem<ushort[]>
    {

        public SecsItemU2(string name)
            : base(FormatCode.U2, name, null)
        { 
        }

        public SecsItemU2(string name, params ushort[] val)
            : base(FormatCode.U2, name, val)
        {
        }

        protected override byte[] GetDataBytes()
        {

            ushort[] vals = this.Value;

            if (vals == null || vals.Length == 0)
            {
                //zero length item
                return null;
            }

            uint byteCount = checked((uint)(sizeof(ushort) * vals.Length));
            byte[] data = new byte[byteCount];

            byte[] valByte;
            int index = 0;
            foreach (ushort v in vals)
            {
                valByte = BitConverter.GetBytes(v);
                Array.Reverse(valByte);
                Array.Copy(valByte, 0, data, index, valByte.Length);
                index += valByte.Length;
            }

            return data;
        }

        protected override void ReadValue(System.IO.MemoryStream reader, uint byteToRead)
        {
            byte[] data = new byte[byteToRead];
            reader.Read(data, 0, data.Length);

            //warning if devine result has fracment
            int valSizeInByte = sizeof(ushort);
            int valCount = (data.Length / valSizeInByte);

            ushort[] vals = new ushort[valCount];

            int index = 0;
            byte[] temp = new byte[valSizeInByte];
            for (int i = 0; i < valCount; i++)
            {
                Array.Copy(data, index, temp, 0, temp.Length);
                Array.Reverse(temp);

                vals[i] = BitConverter.ToUInt16(temp, 0);
                index += valSizeInByte;
            }

            this.Value = vals;
        }
    }
}
