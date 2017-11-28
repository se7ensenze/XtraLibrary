using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsItemF8:SecsItem<double[]>
    {
        public SecsItemF8(string name)
            : base(FormatCode.F8, name, null)
        { 
        
        }

        public SecsItemF8(string name, params double[] val)
            :base(FormatCode.F8, name, val) { 
        
        }

        protected override byte[] GetDataBytes()
        {

            double[] vals = this.Value;

            if (vals == null || vals.Length == 0)
            {
                //zero length item
                return null;
            }

            uint byteCount = checked((uint)(sizeof(double) * vals.Length));
            byte[] data = new byte[byteCount];

            byte[] valByte;
            int index = 0;
            foreach (double v in vals)
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
            int valSizeInByte = sizeof(double);
            int valCount = (data.Length / valSizeInByte);

            double[] vals = new double[valCount];

            int index = 0;
            byte[] temp = new byte[valSizeInByte];
            for (int i = 0; i < valCount; i++)
            {
                Array.Copy(data, index, temp, 0, temp.Length);
                Array.Reverse(temp);

                vals[i] = BitConverter.ToDouble(temp, 0);
                index += valSizeInByte;
            }
            this.Value = vals;                     // 160711 \783 Add Return Value
        }
    }
}
