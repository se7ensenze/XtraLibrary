using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace XtraLibrary.SecsGem
{
    public abstract class SVAttributeMapper
    {
        private EquipmentModel m_Model;
        private Dictionary<object, SVIDMapInfo> m_SVAttributeDic;

        #region "Netested class"

        private class SVIDMapInfo
        {
            internal PropertyInfo PropertyInfo;
            internal SVIDAttribute SVIDInfo;

            internal SVIDMapInfo(PropertyInfo pi, SVIDAttribute svAttr)
            {
                this.PropertyInfo = pi;
                this.SVIDInfo = svAttr;
            }
        }

        #endregion

        public SVAttributeMapper(EquipmentModel model)
        {
            m_SVAttributeDic = new Dictionary<object, SVIDMapInfo>();
            m_Model = model;
            m_Model.SetSVIDMapper(this);
            Map(m_Model);
        }

        private void Map(EquipmentModel model)
        {
            m_SVAttributeDic.Clear();

            Type eqType = model.GetType();
            Type svAttributeType = typeof(SVIDAttribute);
            SVIDAttribute svAttribute;

            PropertyInfo[] props = eqType.GetProperties();
            SVIDMapInfo mi = null;

            foreach (PropertyInfo pi in props)
            {
                if (pi.CanWrite && pi.IsDefined(svAttributeType, true))
                {
                    object[] attrs = pi.GetCustomAttributes(svAttributeType, true);
                    svAttribute = (SVIDAttribute)attrs[0];
                    if (m_SVAttributeDic.ContainsKey(svAttribute.ID))
                    {
                        //throw error
                        throw new Exception(string.Format("Duplicated SVID {0} PropertyName := {1}", svAttribute.ID, pi.Name));
                    }

                    mi = new SVIDMapInfo(pi, svAttribute);

                    m_SVAttributeDic.Add(svAttribute.ID, mi);

                }
            }
        }

        public void ObtainSvValueFromS6F11(SecsMessageBase s6f11)
        {
            SecsItemList secsItem_L3 = (SecsItemList)s6f11.Items[0];
            SecsItem secsItem_DataID = secsItem_L3.Value[0];
            SecsItem secsItem_CEID = secsItem_L3.Value[1];
            SecsItemList secsItem_La = (SecsItemList)secsItem_L3.Value[2];

            SecsItem secsItem_RPTID = null;
            SecsItemList secsItem_Lb = null;

            foreach (SecsItemList secsItem_L2 in secsItem_La.Value)
            {
                secsItem_RPTID = secsItem_L2.Value[0];
                secsItem_Lb = (SecsItemList)secsItem_L2.Value[1];

                for (int index = 0; index < secsItem_Lb.Value.Count; index++)
                {
                    object dfSvid = InternalGetDefinedSVID(secsItem_RPTID, index);
                    if (m_SVAttributeDic.ContainsKey(dfSvid))
                    {
                        SVIDMapInfo mi = m_SVAttributeDic[dfSvid];
                        SetStatusVariable(mi, secsItem_Lb.Value[index]);
                    }
                    else
                    {
                        //throw error
                    }
                }
            }
        }

        private void SetStatusVariable(SVIDMapInfo mi, SecsItem secsItem_SV)
        {
            //2.) Get value from secs item
            object val = null;

            SVIDAttribute svAttr = mi.SVIDInfo;

            if (secsItem_SV is SecsItemU1)
            {
                if (svAttr.Size == 1)
                {
                    val = ((SecsItemU1)secsItem_SV).Value[0];
                }
                else
                {
                    val = ((SecsItemU1)secsItem_SV).Value;
                }
            }
            else if (secsItem_SV is SecsItemU2)
            {
                if (svAttr.Size == 1)
                {
                    val = ((SecsItemU2)secsItem_SV).Value[0];
                }
                else
                {
                    val = ((SecsItemU2)secsItem_SV).Value;
                }
            }
            else if (secsItem_SV is SecsItemU4)
            {
                if (svAttr.Size == 1)
                {
                    val = ((SecsItemU4)secsItem_SV).Value[0];
                }
                else
                {
                    val = ((SecsItemU4)secsItem_SV).Value;
                }
            }
            else if (secsItem_SV is SecsItemU8)
            {
                if (svAttr.Size == 1)
                {
                    val = ((SecsItemU8)secsItem_SV).Value[0];
                }
                else
                {
                    val = ((SecsItemU8)secsItem_SV).Value;
                }
            }
            else if (secsItem_SV is SecsItemI1)
            {
                if (svAttr.Size == 1)
                {
                    val = ((SecsItemI1)secsItem_SV).Value[0];
                }
                else
                {
                    val = ((SecsItemI1)secsItem_SV).Value;
                }
            }
            else if (secsItem_SV is SecsItemI2)
            {
                if (svAttr.Size == 1)
                {
                    val = ((SecsItemI2)secsItem_SV).Value[0];
                }
                else
                {
                    val = ((SecsItemI2)secsItem_SV).Value;
                }
            }
            else if (secsItem_SV is SecsItemI4)
            {
                if (svAttr.Size == 1)
                {
                    val = ((SecsItemI4)secsItem_SV).Value[0];
                }
                else
                {
                    val = ((SecsItemI4)secsItem_SV).Value;
                }
            }
            else if (secsItem_SV is SecsItemI8)
            {
                if (svAttr.Size == 1)
                {
                    val = ((SecsItemI8)secsItem_SV).Value[0];
                }
                else
                {
                    val = ((SecsItemI8)secsItem_SV).Value;
                }
            }
            else if (secsItem_SV is SecsItemBinary)
            {
                if (svAttr.Size == 1)
                {
                    val = ((SecsItemBinary)secsItem_SV).Value[0];
                }
                else
                {
                    val = ((SecsItemBinary)secsItem_SV).Value;
                }
            }
            else if (secsItem_SV is SecsItemAscii)
            {
                val = ((SecsItemAscii)secsItem_SV).Value;
            }
            else if (secsItem_SV is SecsItemBoolean)
            {
                if (svAttr.Size == 1)
                {
                    val = ((SecsItemBoolean)secsItem_SV).Value[0];
                }
                else
                {
                    val = ((SecsItemBoolean)secsItem_SV).Value;
                }
            }
            else
            {
                throw new Exception("Not support type " + secsItem_SV.GetType().Name);
            }

            PropertyInfo pi = mi.PropertyInfo;
            pi.SetValue(m_Model, val, null);
        }

        internal abstract object InternalGetDefinedSVID(SecsItem secsItem_RptID, int index);

    }

    public abstract class SVAttributeMapper<SVID>
        : SVAttributeMapper
    {

        public SVAttributeMapper(EquipmentModel model)
            : base(model) 
        { 
        }

        internal override object InternalGetDefinedSVID(SecsItem secsItem_RptID, int index)
        {
            return GetDefinedSVID(secsItem_RptID, index);
        }

        protected abstract SVID GetDefinedSVID(SecsItem secsItem_RptID, int index);
    }

}
