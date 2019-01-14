using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs
{
    /// <summary>
    /// Описывает отдельную уставку аналогового параметра
    /// </summary>
    class UstValue:IComparable<UstValue>, ICloneable
    {
        public float Value { get; set; }
        public float OldValue { get; set; }
        public bool Used { get; set; }
        public bool Changed { get; set; }
        public bool Different { get; set; }

        public string UstName { get; set; }
        public string Egu { get; set; }

        public byte Format { get; set; }

        // пустой конструктор
        public UstValue() { }

        /// <summary>
        /// Создание экземпляра уставки
        /// </summary>
        /// <param name="value">занчение уставки</param>
        /// <param name="used">уставка используется или нет</param>
        public UstValue(float value, bool used, string ustname, string egu, byte format)
        {
            Value = value;
            Used = used;
            UstName = ustname;
            Egu = egu;
            Format = format;
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
                Format = this.Format
            };
        }
    }
}
