using System;
using System.Collections.Generic;
using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsItemF4:SecsItem<float[]>
    {
        public SecsItemF4(string name)
            : base(FormatCode.F4, name, null)  //160711 \783 Error fix 
        { 
        
        }

        public SecsItemF4(string name, params float[] val)
            : base(FormatCode.F4, name, val)      //160711 \783 Error fix 
        { 
        
        }

        protected override byte[] GetDataBytes()
        {

            float[] vals = this.Value;

            if (vals == null || vals.Length == 0)
            {
                //zero length item
                return null;
            }

            uint byteCount = checked((uint)(sizeof(float) * vals.Length));
            byte[] data = new byte[byteCount];

            byte[] valByte;
            int index = 0;
            foreach (float v in vals)
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
            int valSizeInByte = sizeof(float);
            int valCount = (data.Length / valSizeInByte);

            float[] vals = new float[valCount];

            int index = 0;
            byte[] temp = new byte[valSizeInByte];
            for (int i = 0; i < valCount; i++)
            {
                Array.Copy(data, index, temp, 0, temp.Length);
                Array.Reverse(temp);

                vals[i] = BitConverter.ToSingle(temp, 0);
                index += valSizeInByte;
            }
            this.Value = vals;                     // 160711 \783 Add Return Value
        }
    }
}
