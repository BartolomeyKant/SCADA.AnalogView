using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs
{
    public delegate void OnAnalogValueChange(uint valueCode);
    class AnalogValue
    {
        float adcValue = 0;
        /// <summary>
        /// текущее значение кодов АЦП
        /// </summary>
        public float ADCValue
        {
            get { return adcValue; }
            set
            {
                adcValue = value;
                // пересчет инженерного значения
                if (resultUstavki != null)
                    ENGValue = (resultUstavki.EMax.Value - resultUstavki.EMin.Value) * (adcValue - resultUstavki.ADCMin.Value) /
                                        (resultUstavki.ADCMax.Value - resultUstavki.ADCMin.Value) + resultUstavki.EMin.Value;
                OnAnalogValueChange(1);
            }
        }

        float engValue = 0;
        /// <summary>
        /// Текущее пересчитанное значение инженерной величины
        /// </summary>
        public float ENGValue {
            set { engValue = value; OnAnalogValueChange(2); }
            get { return engValue; }
        }

        float plcValue = 0;
        /// <summary>
        /// Текущее значение в контроллере принятое в работу
        /// </summary>
        public float PLCValue
        {
            set {
                    plcValue = value;
                    OnAnalogValueChange(3);
            }
            get { return plcValue; }
        }

        ushort analogState;
        /// <summary>
        /// Текущее состояние аналогового сигнала
        /// </summary>
        public ushort AnalogState {
            set { analogState = value; OnAnalogValueChange(4); }
            get { return analogState;  }}

        /// <summary>
        /// точность представления кодов АЦП
        /// </summary>
        public byte ADCFormat { set; get; }

        /// <summary>
        /// точноить представления значения аналогового сигнала
        /// </summary>
        public byte ValueFormat { set; get; }

        // ссылка на контейнер с результирующими уставками
        UstavkiContainer resultUstavki;


        /// <summary>
        ///  подписка на измененное значение выполнена
        /// </summary>
        public bool SubscribeComplite { set; get; }

        // обработка изменения контекста уставок
        public void _OnConteinerChanged(UstavkiContainer newUstvakiContainer)
        {
            resultUstavki = newUstvakiContainer;
        }

        public event OnAnalogValueChange AnalogValueChangeEvent;
        void OnAnalogValueChange(uint valueCode)
        {
            AnalogValueChangeEvent?.Invoke(valueCode);
        }

    }
}
