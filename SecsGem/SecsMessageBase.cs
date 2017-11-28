using System;
using System.Collections.Generic;

using System.Text;
using System.Collections;
using System.IO;

namespace XtraLibrary.SecsGem
{
    public abstract class SecsMessageBase
    {
        List<SecsItem> m_Items;
        private byte m_Stream;
        private byte m_Function;
        private bool m_NeedReply;
        private uint  m_TransactionId;
        private ushort m_DeviceId;

        #region "Properties"

        public byte Stream
        {
            get { return m_Stream; }
            internal set
            {
                m_Stream = value;
            }
        }

        public byte Function
        {
            get { return m_Function; }
            internal set
            {
                m_Function = value;
            }
        }

        public bool NeedReply
        {
            get { return m_NeedReply; }
            internal set
            {
                m_NeedReply = value;
            }
        }

        public uint TransactionId
        {
            get {
                return m_TransactionId;
            }
            set
            {
                m_TransactionId = value;
            }
        }

        public List<SecsItem> Items
        {
            get {
                return m_Items;
            }
        }


        public ushort DeviceId
        {
            get { return m_DeviceId; }
            set
            {
                m_DeviceId = value;
            }
        }
			

        #endregion

        #region "Protected methods"

        /// <summary>
        /// Create new Item
        /// </summary>
        /// <param name="s">Stream</param>
        /// <param name="f">Function</param>
        /// <param name="w">W-Bit : if true it mean need reply message</param>
        protected SecsMessageBase(byte s, byte f, bool w)
        {
            m_Stream = s;
            m_Function = f;
            m_NeedReply = w;
            m_Items = new List<SecsItem>();
            m_TransactionId = 0;
        }

        /// <summary>
        /// Incase of Secondary message
        /// </summary>
        /// <param name="priMsg">Primary Message</param>
        protected SecsMessageBase(SecsMessageBase priMsg)
            :this(priMsg.Stream,  (byte)(priMsg.Function + 1), false)
        {
            m_TransactionId = priMsg.TransactionId;
            m_DeviceId = priMsg.DeviceId;            
        }

        /// <summary>
        /// Add secs item
        /// </summary>
        /// <param name="item">secs item</param>
        protected void AddItem(SecsItem item)
        {
            if (!m_Items.Contains(item))
            {
                m_Items.Add(item);
            }            
        }

        /// <summary>
        /// Add Level 1 Item
        /// </summary>
        /// <param name="name">name of the parameter</param>
        /// <param name="itemType">item type</param>
        /// <returns></returns>
        protected SecsItem AddItem(string name, Type itemType)
        {
            if (!itemType.IsSubclassOf(typeof(SecsItem)) || (!itemType.IsAbstract))
            {
                //not support
                throw new Exception("Invalide item type");
            }

            SecsItem item = (SecsItem)Activator.CreateInstance(itemType, name);
            AddItem(item);

            return item;
        }

        #endregion

        #region "Internal methods"

        public void ReadItems(MemoryStream reader)
        {
            //if developer did not define item of this message
            if (m_Items.Count == 0)
            {
                byte[] formatCodeByte = new byte[1];
                SecsItem item;
                FormatCode fc;
                byte lenghtByteLong;

                while (reader.Position < reader.Length)
                {
                    reader.Read(formatCodeByte, 0, formatCodeByte.Length);

                    //move back to format code position because *1
                    reader.Position -= formatCodeByte.Length;

                    fc = FormatCodeHelper.GetFormatCode(formatCodeByte[0], out lenghtByteLong);
                    item = SecsItemFactory.Create("DV" + m_Items.Count.ToString(), fc);
                    //*1 this function is read format code also 
                    item.Read(reader);            

                    AddItem(item);
                }
            }
            //if developer already defined item of this message
            else if (m_Items.Count > 0)
            {
                foreach (SecsItem item in m_Items)
                {
                    item.Read(reader);
                }
            }
        }

        #endregion

    }

}
