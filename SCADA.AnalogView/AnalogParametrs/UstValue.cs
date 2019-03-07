using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs
{
    enum UstCode
    {
        UstMin6 = 1,
        UstMin5,
        UstMin4,
        UstMin3,
        UstMin2,
        UstMin1,
        UstMax1 = 7,
        UstMax2,
        UstMax3,
        UstMax4,
        UstMax5,
        UstMax6,
        ADCMax=101,
        ADCMin,
        EMax=201,
        EMin,
        VPD = 301,
        NPD,
        Hister = 401
    }

    /// <summary>
    /// делегат события изменения уставки
    /// </summary>
    delegate void UstValueChanged();

    /// <summary>
    /// Описывает отдельную уставку аналогового параметра
    /// </summary>
    class UstValue:IComparable<UstValue>, ICloneable
    {
        public float Value { get; set; }
        public float OldValue { get; set; }
        public float DiffValue { get; set; }
        public bool Used { get; set; }
        public bool Changed { get; set; }
        public bool Different { get; set; }

        public string UstName { get; set; }
        public string Egu { get; set; }

        public byte Format { get; set; }

        /// <summary>
        /// Кодовый идентификатор уставки
        /// 1..12 - технологические уставки
        /// 101..102 - кода АЦП
        /// 201..202 - Инженерные величины
        /// 301..302 - Пределы достоверности
        /// 401 - гистерезис
        /// </summary>
        public UstCode UstCode { get; set; }

        /// <summary>
        /// Событие изменения значений уставки
        /// </summary>
        public event UstValueChanged OnUstValueChangedEvent;
        void OnUstValueChanged()
        {
            OnUstValueChangedEvent?.Invoke();
        }

        // пустой конструктор
        public UstValue() { }

        /// <summary>
        /// Создание экземпляра уставки
        /// </summary>
        /// <param name="value">занчение уставки</param>
        /// <param name="used">уставка используется или нет</param>
        /// <param name="ustname">Наименование уставки</param>
        ///  <param name="egu">Единица измерения уставки  </param>
        ///  <param name="format">Количество знаков после запятой для отображения </param>
        ///  <param name="ustCode"> Кодовый идентификатор уставки </param>
        public UstValue(float value, bool used, string ustname, string egu, byte format, UstCode ustCode)
        {
            Value = value;
            Used = used;
            UstName = ustname;
            Egu = egu;
            Format = format;
            UstCode = ustCode;
        }

        // для выполнения сравнения с другими уставками
        public int CompareTo(UstValue val)
        {
            if (val != null)
            {
                if (Value > val.Value)
                {
                    return 1;
                }
                else if (Value < val.Value)
                {
                    return -1;
                }
                else if (Used != val.Used)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                throw new Exception("Не возможно выполнить сравнение текущий уставки. Передан не правильный объект для сравнения");
            }
        }

        public object Clone()
        {
            return new UstValue()
            {
                Value = this.Value,
                OldValue = this.OldValue,
                Used = this.Used,
                Changed = this.Changed,
                Different = this.Different,
                UstName = this.UstName,
                Egu = this.Egu,
                Format = this.Format,
                UstCode = this.UstCode
            };
        }

        /// <summary>
        /// Задание нового значения уставки
        /// </summary>
        /// <param name="newValue">Новое значение</param>
        public void SetNewValue(float newValue)
        {
            newValue = (float)Math.Round(newValue, Format);         // Округление к текущему формату

            if ((!Changed) && (Value != newValue))
            {
                OldValue = Value;
                Changed = true;
            }
            else if ((Changed) && (newValue == OldValue))
                Changed = false;

            Value = newValue;
            OnUstValueChanged();
        }
        /// <summary>
        /// Обновление значения 
        /// </summary>
        public void UpdateValue()
        {
            OnUstValueChanged();
        }
    }
}
