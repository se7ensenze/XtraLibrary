using System;
using System.Collections.Generic;

using System.Text;
using System.Collections;

namespace XtraLibrary.SecsGem
{
    public class SecsItemList:SecsItem<List<SecsItem>>
    {

        public SecsItemList(string name) 
            : base(FormatCode.LIST, name, new List<SecsItem>()) 
        {            
        }

        protected override void ReadValue(System.IO.MemoryStream reader, uint itemToRead)
        {
            List<SecsItem> elements = this.Value;

            if (elements.Count == 0) //not fixed item list
            {
                long readItemCount = 0;

                byte[] formatCodeByte = new byte[1];
                byte lenghtByteLong = 0;

                FormatCode fc;
                SecsItem item = null;

                while (readItemCount < itemToRead)
                {
                    reader.Read(formatCodeByte, 0, formatCodeByte.Length);

                    //move back to format code position
                    reader.Position -= formatCodeByte.Length;

                    fc = FormatCodeHelper.GetFormatCode(formatCodeByte[0], out lenghtByteLong);
                    item = SecsItemFactory.Create("DV" + elements.Count.ToString(), fc);
                    item.Read(reader);
                    elements.Add(item);
                    readItemCount += 1;
                } //while (readItemCount < itemToRead)
            }
            else if (itemToRead == elements.Count) //fixed item list
            {
                //class defination error
                foreach (SecsItem item in elements)
                {
                    item.Read(reader);
                }
            }
            else
            { 
                //invalid define 
                throw new Exception("Defined item count is not equal to item to read");
            }
            
        }

        protected override int GetListItemLength()
        {
            return this.Value.Count;
        }

        protected override byte[] GetDataBytes()
        {
            List<SecsItem> elements = this.Value ;
            List<byte[]> itemBytesList = new List<byte[]>();

            long totalByte = 0;
            byte[] tmp;
            foreach (SecsItem item in elements)
            {
                tmp = item.ToBytes();
                totalByte += tmp.LongLength;
                itemBytesList.Add(tmp);
            }

            byte[] data = new byte[totalByte];
            long index = 0;

            for (int i = 0; i < itemBytesList.Count; i++)
            {
                tmp = itemBytesList[i];
                Array.Copy(tmp, 0, data, index, tmp.Length);
                index += tmp.Length ;
            }
            
            itemBytesList.Clear();
            itemBytesList = null;

            return data;
        }

        #region "For easy to use"

        public void AddItem(SecsItem item)
        {
            this.Value.Add(item);
        }

        public void Clear()
        {
            this.Value.Clear();
        }

        #endregion
    }
}
