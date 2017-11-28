using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace XtraLibrary.SecsGem
{
    public class EquipmentModel
    {
        private string m_Name;
        private GemOption m_Connection;
        
        public EquipmentModel(string eqName)
        {
            m_Connection = new GemOption();
            m_Name = eqName;
        }

        #region "Properties"

        public string Name
        {
            get {
                return m_Name;
            }
            set {
                m_Name = value;
            }
        }

        public GemOption Connection
        {
            get
            {
                return m_Connection;
            }
            set
            {
                m_Connection = value;
            }
        }

        #endregion
        
    }
}
