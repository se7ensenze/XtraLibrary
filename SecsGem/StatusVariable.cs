using System;
using System.Collections.Generic;

using System.Text;

namespace XtraLibrary.SecsGem
{
    public class StatusVariable
    {
        protected object m_ID;
        protected object m_Value;

        public object ID
        {
            get
            {
                return m_ID;
            }
            set
            {
                m_ID = value;
            }
        }

        public object Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                m_Value = value;
            }
        }
    }

    public class StatusVariable<T, V>
        : StatusVariable
    {
        public new T ID
        {
            get {
                return (T)m_ID;
            }
            set {
                m_ID = value;
            }
        }

        public new V Value
        {
            get {
                return (V)m_Value;
            }
            set
            {
                m_Value = value;
            }
        }
    }
}
