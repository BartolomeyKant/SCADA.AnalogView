using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.AnalogView.HistoriacalData
{
    /// <summary>
    /// буффер хранящий текущие значения исторических данных
    /// </summary>
    class HistorianDataPointBuffer:IEnumerable<HistoricalDataPoint>, IEnumerator<HistoricalDataPoint>
    {
        HistoricalDataPoint[] buffer;
        uint _capacity;                     // емкость буфера
        uint _start;                        // текущая начальная точка буффера
        uint _size;                         // текущее количество элементов в буфере

        uint _cursor;                          // текущая позиция курсора

        /// <summary>
        /// емкость буфера
        /// </summary>
        public uint Capacity
        {
            get { return _capacity; }
        }
        /// <summary>
        /// количество элментов в буфере
        /// </summary>
        public uint Size
        {
            get { return _size; }
        }

        public HistoricalDataPoint Current => GetValue(_cursor);

        object IEnumerator.Current => GetValue(_cursor);

        // получение элемента по индексу
        public HistoricalDataPoint this[uint index]
            {
                get { return this.GetValue(index); }
            }
        /// <summary>
        /// Создание буфера для исторических точек
        /// </summary>
        /// <param name="Capacity"></param>
        public HistorianDataPointBuffer(uint Capacity)
        {
            _capacity = Capacity;
            _start = 0;
            _size = 0;
            buffer = new HistoricalDataPoint[Capacity];
        }

        public IEnumerator<HistoricalDataPoint> GetEnumerator()
        {
            for (uint i = 0; i < _size; i++)
            {
                yield return GetValue(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (uint i = 0; i < _size; i++)
            {
                yield return GetValue(i);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            _cursor = (_cursor + 1) % _capacity;
            if (_cursor >= (_start + _size) % _capacity)
            {
                return false;
            }
            return true;
        }

        public void Reset()
        {
            _cursor = _start;
        }

        /// <summary>
        /// получение значения по индексу
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        HistoricalDataPoint GetValue(uint index)
        {
            if (index >= _size)
            {
                throw new IndexOutOfRangeException($"Запрашиваемый index:{index} превышает размер буфера {_size}");
            }
            uint actualIndex = (_start + index) % _capacity;        // смещение индекса по кольцевому буферу
            return buffer[actualIndex];                             // возвращаем объект по полученному индексу
        }

        /// <summary>
        /// добавление нового значения в буфер
        /// </summary>
        /// <param name="value"></param>
        public void AddValue(HistoricalDataPoint value)
        {
            if (value != null)              // добавляются только значения не null
            {
                uint newIndex = (_start + _size) % _capacity;
                if (_size < _capacity)              // если размер не превышает емкость, то увеличиваем размер
                    _size++;
                else
                    _start = (_start + 1) % _capacity;                       // сдвигаем точку указатель начала
                buffer[newIndex] = value;
            }
        }

        /// <summary>
        /// добавить в буфера сразу массив новых значений
        /// при переполнении значения будут потеряны
        /// </summary>
        /// <param name="values"></param>
        public void AddRange(HistoricalDataPoint[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                this.AddValue(values[i]);
            }
        }
    }
}
