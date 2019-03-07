using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SCADA.Logging;

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
                ustCont.UstValues.Add((UstValue)UstValues[i].Clone());
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

        bool CompareValues(UstValue one, UstValue another)
        {
            if (one.CompareTo(another) != 0)            // если значения не равны
            {
                Logger.AddMessages($"Уставка {one.UstName} имеет различное значение в контроллере и в архиве");
                one.Different = true;
                another.Different = true;
                one.DiffValue = another.Value;
                another.DiffValue = one.Value;
                // обновление отображения значений
                another.UpdateValue();
                one.UpdateValue();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Сравнение с другим контейнером уставок
        /// </summary>
        /// <param name="other">Другой контейнер уставок</param>
        /// <returns>При отличии возвращает true</returns>
        public bool Compare(UstavkiContainer other)
        {
            Logger.AddMessages("Выполняется сравнение уставок");
            bool flDifferent = false;
            try
            {
                flDifferent = flDifferent | CompareValues(ADCMax, other.ADCMax);
                flDifferent = flDifferent | CompareValues(ADCMin, other.ADCMin);
                flDifferent = flDifferent | CompareValues(EMax, other.EMax);
                flDifferent = flDifferent | CompareValues(EMin, other.EMin);
                flDifferent = flDifferent | CompareValues(NPD, other.NPD);
                flDifferent = flDifferent | CompareValues(VPD, other.VPD);
                flDifferent = flDifferent | CompareValues(Hister, other.Hister);

                for (int i = 0; i < 12; i++)
                {
                    flDifferent = flDifferent | CompareValues(UstValues[i], other.UstValues[i]);
                }
                if (flDifferent)
                {
                    Logger.AddMessages("Уставки архива и контроллера отличаются");
                }
            }
            catch (Exception e)
            {
                throw new Exception("При сравнении уставок возникло исключение", e);
            }
            return flDifferent;
        }

        /// <summary>
        /// Сброс флагов сотосния уставок
        /// </summary>
        public void ClearState()
        {
            for(int i =0; i < 12; i++)
            {
                UstValues[i].Changed = false;
                UstValues[i].Different = false;
                UstValues[i].UpdateValue();
            }
            ADCMax.Changed = false; ADCMax.Different = false; ADCMax.UpdateValue();
            ADCMin.Changed = false; ADCMin.Different = false; ADCMin.UpdateValue();
            EMax.Changed = false; EMax.Different = false; EMax.UpdateValue();
            EMin.Changed = false; EMin.Different = false; EMin.UpdateValue();
            VPD.Changed = false; VPD.Different = false; VPD.UpdateValue();
            NPD.Changed = false; NPD.Different = false; NPD.UpdateValue();
            Hister.Changed = false; Hister.Different = false; Hister.UpdateValue();
        }
    }
}
