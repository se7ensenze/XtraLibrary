using System;
using System.Collections.Generic;

using System.Text;
using System.IO;

namespace XtraLibrary.SecsGem
{
    public abstract class SecsItem
    {
        private string m_Name;
        public string Name
        {
            get { return m_Name; }
        }
			
        protected object m_Value;
        public object Value
        {
            get { return m_Value; }
            set
            {
                m_Value = value;
            }
        }

        private FormatCode m_Format;
        public FormatCode Format
        {
            get { return m_Format; }
        }
		
        /// <summary>
        /// Data include header and length byte
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ToBytes();

        internal SecsItem(FormatCode fm, string name)
            : this(fm, name, null)
        {
        }

        internal SecsItem(FormatCode fm, string name, object value)
        {
            m_Format = fm;
            m_Name = name;
            m_Value = value;
        }

        public abstract void Read(MemoryStream reader);

    }

    public abstract class SecsItem<T>:SecsItem
    {
        public new T Value
        {
            get { return (T)m_Value; }
            set
            {
                m_Value = value;
            }
        }

        public override void Read(MemoryStream reader)
        {
            byte[] formatCode = new byte[1];
            //read for check
            reader.Read(formatCode, 0, formatCode.Length);

            byte lengthByteLong = 0;
            FormatCode code = FormatCodeHelper.GetFormatCode(formatCode[0], out lengthByteLong);

            if (this.Format != code)
            {
                //wrong format : throw exception   
                throw new Exception("User defined is " + this.Format .ToString() +
                    " but data format is " + code.ToString());
            }

            byte[] readLengthByte = new byte[lengthByteLong];
            reader.Read(readLengthByte, 0, readLengthByte.Length);

            //readLengthByte { 03, 5C, 9A }
            //prefer { 9A, 5C, 03, 00 }

            //temp { 0, 0, 0, 0}
            byte[] temp = new byte[4];

            //temp { 0, 03, 5C, 9A}
            Array.Copy(readLengthByte, 0, temp, temp.Length - readLengthByte.Length, readLengthByte.Length);

            //temp { 9A, 5C, 03, 00}
            Array.Reverse(temp);
            
            uint byteToRead = BitConverter.ToUInt32(temp, 0);

            //System.Diagnostics.Debug.Print("Format := " + code.ToString() + ", Length := " + byteToRead.ToString());

            ReadValue(reader, byteToRead);
        }

        protected abstract void ReadValue(MemoryStream reader, uint byteToRead);

        public SecsItem(FormatCode fm, string name)
            : base(fm, name)
        { 
            
        }

        public SecsItem(FormatCode fm, string name, T val)
            : base(fm, name, val)
        {            
        }

        /// <summary>
        /// Only [Data] except item header [ FormatCode ][Length Byte Long][Data]
        /// </summary>
        /// <returns>[Data] array</returns>
        protected abstract byte[] GetDataBytes();

        /// <summary>
        /// For LIST only
        /// </summary>
        /// <returns></returns>
        protected virtual int GetListItemLength() {
            return 0;
        }

        public override byte[] ToBytes()
        {

            byte[] data = GetDataBytes();

            uint lengthByteOrElementCount = checked(
                (Format == FormatCode.LIST) ? (uint)GetListItemLength() : 
                (data != null) ? (uint)data.Length : 0);
            //** in case of LIST lengt byte long determine bye item count
            
            //length byte long 
            byte lbl = 0;
            if (lengthByteOrElementCount == 0 || lengthByteOrElementCount <= 0xFF)
            {
                lbl = 1;
            }
            else if (lengthByteOrElementCount <= 0xFFFF)
            {
                lbl = 2;
            }
            else if (lengthByteOrElementCount <= 0xFFFFFF)
            {
                lbl = 3;
            }
            else
            {
                throw new Exception("Data length is over " + (0xFFFFFF).ToString());
            }

            byte[] itemBytes = new byte[1 + lbl + (data != null ? data.Length : 0)]; // 1 = format code byte

            int index = 0;
            //Format Code
            itemBytes[index] = (byte)(((byte)Format << 2) | lbl);
            index = +1; //1 byte format code

            //length byte
            //if btr = 10
            byte[] lengthByte = BitConverter.GetBytes(lengthByteOrElementCount);
            //lengthByte = [0x0A, 0x00, 0x00, 0x00]
            Array.Reverse(lengthByte);
            //lengthByte = [0x00, 0x00, 0x00, 0x0A]
            Array.Copy(lengthByte, lengthByte.Length - lbl, itemBytes, index, lbl);
            index += lbl; //length byte long
            //data
            if (data != null && data.Length > 0)
            {
                Array.Copy(data, 0, itemBytes, index, data.Length);
            }
            return itemBytes;
        }
    }
}
