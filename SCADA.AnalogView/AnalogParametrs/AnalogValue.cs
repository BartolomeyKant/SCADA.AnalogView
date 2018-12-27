using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs
{
    class AnalogValue
    {
        float adcValue;
        /// <summary>
        /// текущее значение кодов АЦП
        /// </summary>
        public float ADCValue
        {
            get {return adcValue; }
            set
            {
                adcValue = value;
                // пересчет инженерного значения
                if(resultUstavki != null)
                    engValue = (resultUstavki.EMax.Value - resultUstavki.EMin.Value) * (adcValue - resultUstavki.ADCMin.Value) / 
                                        (resultUstavki.ADCMax.Value - resultUstavki.ADCMin.Value) + resultUstavki.EMin.Value;
            }
        }
        float engValue;
        /// <summary>
        /// Текущее пересчитанное значение инженерной величины
        /// </summary>
        public float ENGValue { get { return engValue; } }
        /// <summary>
        /// Текущее значение в контроллере принятое в работу
        /// </summary>
        public float PLCValue { get; set; }

        // ссылка на контейнер с результирующими уставками
        UstavkiContainer resultUstavki;


        // обработка изменения контекста уставок
        public void _OnConteinerChanged(UstavkiContainer newUstvakiContainer)
        {
            resultUstavki = newUstvakiContainer;
        }

    }
}
