using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs
{
    class AnalogValue
    {
        double adcValue;
        /// <summary>
        /// текущее значение кодов АЦП
        /// </summary>
        public double ADCValue
        {
            get {return adcValue; }
            set
            {
                adcValue = value;
                // пересчет инженерного значения
                if(resultUstavki != null)
                    ENGValue = (resultUstavki.EMax.Value - resultUstavki.EMin.Value) * (adcValue - resultUstavki.ADCMin.Value) / 
                                        (resultUstavki.ADCMax.Value - resultUstavki.ADCMin.Value) + resultUstavki.EMin.Value;
            }
        }

        double engValue;
        /// <summary>
        /// Текущее пересчитанное значение инженерной величины
        /// </summary>
        public double ENGValue {
            get { return engValue; }
            set { engValue = value; }
        }

        double plcValue;
        /// <summary>
        /// Текущее значение в контроллере принятое в работу
        /// </summary>
        public double PLCValue {
            get { return plcValue; }
            set { plcValue = value; } }

        uint analogState;
        /// <summary>
        /// Текущее состояние аналогового сигнала
        /// </summary>
        public uint AnalogState {
            get { return analogState;  }
            set { analogState = value;  } }


        // ссылка на контейнер с результирующими уставками
        UstavkiContainer resultUstavki;


        // обработка изменения контекста уставок
        public void _OnConteinerChanged(UstavkiContainer newUstvakiContainer)
        {
            resultUstavki = newUstvakiContainer;
        }

    }
}
