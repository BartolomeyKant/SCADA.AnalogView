using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs
{
    /// <summary>
    /// Визуальное представление уставок
    /// </summary>
    class UstContainerViewModel
    {
        UstavkiContainer container;
        public UstavkiContainer Container
        {
            set
            {
                container = value;
                CreateNewUstList();
                // переинициализация остальных уставок
                ADCMax.Controller = container.ADCMax;
                ADCMin.Controller = container.ADCMin;
                EMax.Controller = container.EMax;
                EMin.Controller = container.EMin;
                VPD.Controller = container.VPD;
                NPD.Controller = container.NPD;
                Hister.Controller = container.Hister;
            }
        }

        List<UstViewModel> ustValues;
        public List<UstViewModel> UstValues
        {
            get { return ustValues; }
            set { ustValues = value; }
        }
        public UstViewModel ADCMax;
        public UstViewModel ADCMin;
        public UstViewModel EMax;
        public UstViewModel EMin;
        public UstViewModel VPD;
        public UstViewModel NPD;
        public UstViewModel Hister;

        /// <summary>
        /// Создание модели представления уставок
        /// </summary>
        /// <param name="container">Текущий контейнер уставок</param>
        public UstContainerViewModel(UstavkiContainer container)
        {
            ustValues = new List<UstViewModel>(12);
            // создание уставок
            for (int i = 0; i < 12; i++)
            {
                ustValues.Add(new UstViewModel());
            }

            ADCMax = new UstViewModel();
            ADCMin = new UstViewModel();
            EMax = new UstViewModel();
            EMin = new UstViewModel();
            VPD = new UstViewModel();
            NPD = new UstViewModel();
            Hister = new UstViewModel();

            Container = container;
        }

        /// <summary>
        /// Обработчик события изменения контейнера
        /// </summary>
        /// <param name="newContainer">ссылка на новый контейнер</param>
        public void OnContextChange(UstavkiContainer newContainer)
        {
            Container = newContainer;
        }

        // переинициализация набора уставок
        void CreateNewUstList()
        {
            for (int i = 0; i < ustValues.Count; i++)
            {
                ustValues[i].Controller = container.UstValues[11-i];
            }
        }

    }
}
