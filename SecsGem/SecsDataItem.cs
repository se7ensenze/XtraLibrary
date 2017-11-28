using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class SecsDataItem
    {
        private string m_Name;
        private FormatCode m_Format;
        private FormatCode[] m_PosibleFormats;

        private int m_ItemLength;

        public string Name
        {
            get {
                return m_Name;
            }
        }

        public FormatCode Format
        {
            get {
                return m_Format;
            }
            set
            {
                bool matched = false;
                foreach (FormatCode f in m_PosibleFormats)
                {
                    if (f == value)
                    {
                        matched = true;
                    }
                }
                if (!matched)
                {
                    throw new Exception("Not posible format");
                }
                m_Format = value;
            }
        }
 
        public SecsDataItem(string name, FormatCode fCode, int itemLength, params FormatCode[] possibleFormats)
        {
            m_Name = name;
            m_Format = fCode;
            m_ItemLength = itemLength;
            m_PosibleFormats = possibleFormats;
        }

    }
}
