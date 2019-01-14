using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.AnalogParametrs
{
    /// <summary>
    ///  Опсиание отображение аналогового параметра
    /// </summary>
    class AnalogViewModel
    {
        AnalogParamsController analogController;


        UstContainerViewModel ustContainer;
        /// <summary>
        /// модель представления уставок
        /// </summary>
        public UstContainerViewModel UstContainer
        {
            get
            {
                return ustContainer;
            }
        }

        /// <summary>
        /// наименование аналогового сигнала
        /// </summary>
        public string Name
        {
            get { return analogController.CommonParams.Name; }
        }
        /// <summary>
        /// Тег аналогового сигнала
        /// </summary>
        public string Tag
        {
            get { return analogController.CommonParams.Tag; }
        }


        /// <summary>
        /// Создание объекта пердставления аналогового параметра
        /// </summary>
        /// <param name="controller"></param>
        public AnalogViewModel(AnalogParamsController controller)
        {
            analogController = controller;
            ustContainer = new UstContainerViewModel(controller.Ustavki);           // преставление контейнера уставок
            analogController.OnUstConteinerChanged += ustContainer.OnContextChange;        // привязка к событию изменния контекста уставок

        }
    }
}
