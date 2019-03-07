
using System.ComponentModel;
using SCADA.Logging;

namespace SCADA.AnalogView.AnalogParametrs
{
    /// <summary>
    /// класс пердставления текущего значения аналогового сигнала
    /// </summary>
    class ValueViewModel : INotifyPropertyChanged
    {
        AnalogValue analogValue;

        ValueView adcValue;
        /// <summary>
        /// текущее значение кода АЦП
        /// </summary>
        public ValueView ADCValue
        {
            get {return adcValue; }
        }

        ValueView engValue;
        /// <summary>
        /// текущее значение пересчитанного инженерного значения
        /// не зависит от имитации
        /// </summary>
        public ValueView EngValue
        {
            get { return engValue;  }
        }

        /// <summary>
        /// текущее состояние аналогового сигнала
        /// отображается если нет имитации
        /// </summary>
        public byte EngValueState
        {
            get {
                if (IsImit)
                {
                    return 0;
                }
                return (byte)(analogValue.AnalogState & 7);
            }
        }

        /// <summary>
        /// текущее состояние аналогового сигнала 
        /// отображается только во время имитации
        /// </summary>
        public byte PLCValueState
        {
            get {
                if (!IsImit)
                {
                    return 0;
                }
                return (byte)(analogValue.AnalogState & 7);
            }
        }

        /// <summary>
        /// Сигнала в имитации или нет ( в 10 бите )
        /// </summary>
        public bool IsImit
        {
            get { return (analogValue.AnalogState & (1 << 10)) > 0; }
        }

        ValueView plcValue;
        /// <summary>
        /// текущее значение в контроллере
        /// текущее имитированное значение
        /// </summary>
        public ValueView PLCValue
        {
            get { return plcValue; }
        }

        /// <summary>
        /// Единицы измерения для инженерных значений
        /// </summary>
        public string Egu { set; get; }
        /// <summary>
        /// Единицы измерения для кодов АЦП
        /// </summary>
        public string ADCEgu { set; get; }

        public void ValueChangeEventHandler(uint valueCode)
        {
            switch (valueCode)
            {
                case 1:
                    adcValue.Value = analogValue.ADCValue.ToString("f" + analogValue.ADCFormat);
                    break;
                case 2:
                    engValue.Value = analogValue.ENGValue.ToString("f" + analogValue.ValueFormat);
                    break;
                case 3:
                    plcValue.Value = analogValue.PLCValue.ToString("f" + analogValue.ValueFormat);
                    break;
                case 4:
                    if (IsImit)
                    {
                        plcValue.State = StateConvert(analogValue.AnalogState);
                        plcValue.IsReadOnly = false;
                        engValue.State = 0;
                    }
                    else
                    {
                        plcValue.State = 0;
                        engValue.State = StateConvert(analogValue.AnalogState);
                        plcValue.IsReadOnly = true;
                    }
                    OnPropertyChanged("IsImit");
                    break;
                default:
                    Logger.AddWarning($"Неизвестный код значения - {valueCode}");
                    break;
            }
        }

        /// преобразуем состояние в кодировку цвета 0 - серый, 1 - зеленый, 2 - желтый, 3 - красный
        byte StateConvert(uint State)
        {
            byte tmpState;
            
            tmpState = (byte)(State & 7);
            if (tmpState >= 6 || tmpState == 0) // если недостоверность
                tmpState = 0;   
            else if (tmpState == 1 || tmpState == 5)        // еслми ававрийный
                tmpState = 3;
            else if (tmpState == 2 || tmpState == 4)        // если предупредительный
                tmpState = 2;
            else if (tmpState == 3)                         // если норма
                tmpState = 1;

            return tmpState;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public ValueViewModel(AnalogValue value, string ADCEgu, string Egu)
        {
            analogValue = value;
            this.ADCEgu = ADCEgu;
            this.Egu = Egu;
            // пердставления для значений
            adcValue = new ValueView() { Egu = this.ADCEgu };
            engValue = new ValueView() { Egu = this.Egu };
            plcValue = new ValueView() { Egu = this.Egu };

            analogValue.AnalogValueChangeEvent += ValueChangeEventHandler;
        }
    }


    /// <summary>
    ///  класс представление текущего значения сигнала
    /// </summary>
    class ValueView: INotifyPropertyChanged
    {
        string value;
        public string Value
        {
            set {  this.value = value; OnPropertyChanged("Value"); }
            get { return value; }
        }

        byte state;
        public byte State
        {
            set {state = value; OnPropertyChanged("State"); }
            get { return state; }
        }
        string egu;
        public string Egu
        {
            set { egu = value; }
            get { return egu; }
        }

        bool isReadOnly;
        public bool IsReadOnly
        {
            set { isReadOnly = value; OnPropertyChanged("IsReadOnly"); }
            get { return isReadOnly; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
