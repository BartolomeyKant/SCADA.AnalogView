using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SCADA.AnalogView.AnalogParametrs
{
    /// <summary>
    /// Визуальное представление отдульной уставки
    /// </summary>
    class UstViewModel:INotifyPropertyChanged
    {
        UstValue controller;
        public UstValue Controller
        {
            set
            {
                controller = value;
                format = controller.Format;
                // вызов события изменения свойств
                OnPropertyChanged("Value");
                OnPropertyChanged("OldValue");
                OnPropertyChanged("Name");
                OnPropertyChanged("Egu");
                OnPropertyChanged("Different");
                OnPropertyChanged("Changed");
            }
        }

        byte format;

        public string Value
        {
            get
            {
                return controller.Value.ToString($"f{format}");
            }
        }
        public string OldValue
        {
            get { return controller.OldValue.ToString($"f{format}"); }
        }
        public string Name
        {
            get { return controller.UstName; }
        }
        public string Egu
        {
            get { return controller.Egu; }
        }
        public bool Different
        {
            get { return controller.Different; }
        }
        public bool Changed
        {
            get { return controller.Changed; }
        }

        public bool Used
        {
            get { return controller.Used;  }
        }

        // реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

    }
}
