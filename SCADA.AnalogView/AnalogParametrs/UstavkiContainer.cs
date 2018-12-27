using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs
{
    /// <summary>
    /// Содержит перчень все уставок используемых при работе OIP
    /// </summary>
    class UstavkiContainer: ICloneable
    {
        /// <summary>
        /// Технологические уставки
        /// </summary>
        public List<UstValue> UstValues = new List<UstValue>(12);              // технологические уставки
        //общие уставки
        public UstValue ADCMin;  
        public UstValue ADCMax;
        public UstValue EMax;
        public UstValue EMin;
        public UstValue NPD;
        public UstValue VPD;
        public UstValue Hister;


        public object Clone()
        {
            UstavkiContainer ustCont = new UstavkiContainer() ;
            ustCont.UstValues = new List<UstValue>(12);
            for (int i = 0; i < ustCont.UstValues.Capacity; i++)
            {
                ustCont.UstValues[i] = (UstValue)UstValues[i].Clone();
            }
            ustCont.ADCMax = (UstValue)ADCMax.Clone();
            ustCont.ADCMin = (UstValue)ADCMin.Clone();
            ustCont.EMax = (UstValue)EMax.Clone();
            ustCont.EMin = (UstValue)EMin.Clone();
            ustCont.VPD = (UstValue)VPD.Clone();
            ustCont.NPD = (UstValue)NPD.Clone();
            ustCont.Hister = (UstValue)Hister.Clone();
            return ustCont;
        }
    }
}
