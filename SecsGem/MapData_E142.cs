using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace XtraLibrary.SecsGem
{
    public class MapData_E142
    {
        #region "Nested Class"

        [XmlElement("Layout")]
        public class Layout
        {
            private string m_ID;
            private string m_DefaultUnits;
            private bool m_TopLevel;
            private ValuePairXY m_Dimension;
            private ValuePairXY m_DeviceSize;
            private List<ChildLayout> m_ChildLayoutList;

            [XmlAttribute("LayoutId")]
            public string ID
            {
                get { return m_ID; }
                set
                {
                    m_ID = value;
                }
            }

            [XmlAttribute("DefaultUnits")]
            public string DefaultUnits
            {
                get { return m_DefaultUnits; }
                set
                {
                    m_DefaultUnits = value;
                }
            }

            [XmlAttribute("TopLevel")]
            public bool TopLevel
            {
                get { return m_TopLevel; }
                set
                {
                    m_TopLevel = value;
                }
            }

            [XmlElement("Dimension")]
            public ValuePairXY Dimension
            {
                get { return m_Dimension; }
                set
                {
                    m_Dimension = value;
                }
            }

            [XmlElement("DeviceSize")]
            public ValuePairXY DeviceSize
            {
                get { return m_DeviceSize; }
                set
                {
                    m_DeviceSize = value;
                }
            }

            [XmlElement("ChildLayouts")]
            public List<ChildLayout> ChildLayoutList
            {
                get { return m_ChildLayoutList; }
                set
                {
                    m_ChildLayoutList = value;
                }
            }
			
			
        }

        [XmlElement("ChildLayout")]
        public class ChildLayout
        {
            public ChildLayout() { }

            private string m_ID;

            [XmlAttribute("LayoutId")]
            public string ID
            {
                get { return m_ID; }
                set
                {
                    m_ID = value;
                }
            }
			
        }

        public class ValuePairXY
        {
            public ValuePairXY() { }

            private int m_X;
            private int m_Y;

            [XmlAttribute("X")]
            public int X
            {
                get { return m_X; }
                set
                {
                    m_X = value;
                }
            }

            [XmlAttribute("Y")]
            public int Y
            {
                get { return m_Y; }
                set
                {
                    m_Y = value;
                }
            }
			
        }

        #endregion

        private MapData_E142() { }
        
        private List<Layout> m_LayoutList;

        [XmlElement("Layouts")]
        public List<Layout> LayoutList
        {
            get { return m_LayoutList; }
            set
            {
                m_LayoutList = value;
            }
        }
		
        //<?xml version="1.0" encoding="utf-8"?>
        //<MapData xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="urn:semi-org:xsd.E142-1.V1005.SubstrateMap">
        //  <Layouts>
        //    <Layout LayoutId="StripLayout" DefaultUnits="mm" TopLevel="True">
        //      <Dimension X="1" Y="1" />
        //      <DeviceSize X="170" Y="200" />
        //      <ChildLayouts>
        //        <ChildLayout LayoutId="ChipLayout" />
        //      </ChildLayouts>
        //    </Layout>
        //    <Layout LayoutId="ChipLayout" DefaultUnits="mm">
        //      <Dimension X="17" Y="14" />
        //      <LowerLeft X="4.5" Y="42" />
        //      <DeviceSize X="" Y="" />
        //      <StepSize X="50" Y="15" />
        //      <Z Order="0" Height="500" Units="um" />
        //    </Layout>
        //  </Layouts>
        //  <Substrates>
        //    <Substrate SubstrateType="Strip" SubstrateId="P1AAA0003" />
        //  </Substrates>
        //  <SubstrateMaps>
        //    <SubstrateMap SubstrateType="Strip" SubstrateId="P1AAA0003" LayoutSpecifier="StripLayout/ChipLayout" Orientation="180">
        //      <Overlay MapName="BinCodeMap" MapVersion="1">
        //        <BinCodeMap BinType="Integer2" NullBin="FFFF">
        //          <BinDefinitions>
        //            <BinDefinition BinCode="0002" BinCount="0" BinQuality="Pass" BinDescription="Good" Pick="false" />
        //            <BinDefinition BinCode="0008" BinCount="0" BinQuality="Fail" BinDescription="Chipping bottom" Pick="false" />
        //            <BinDefinition BinCode="0001" BinCount="0" BinQuality="Fail" BinDescription="Missing Reference" Pick="false" />
        //            <BinDefinition BinCode="0009" BinCount="0" BinQuality="Fail" BinDescription="Scrap" Pick="false" />
        //            <BinDefinition BinCode="000C" BinCount="0" BinQuality="Fail" BinDescription="FM" Pick="false" />
        //            <BinDefinition BinCode="0006" BinCount="0" BinQuality="Fail" BinDescription="MarkingError" Pick="false" />
        //          </BinDefinitions>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //          <BinCode>FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF</BinCode>
        //        </BinCodeMap>
        //      </Overlay>
        //    </SubstrateMap>
        //  </SubstrateMaps>
        //</MapData>

        
    }
}
