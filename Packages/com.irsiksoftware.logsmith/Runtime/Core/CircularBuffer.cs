using System;
using System.Collections.Generic;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Circular buffer with fixed capacity for efficient log storage.
    /// </summary>
    public class CircularBuffer<T>
    {
        private readonly T[] _buffer;
        private int _start;
        private int _count;
        private readonly int _capacity;
        private readonly object _lock = new object();

        public int Count
        {
            get { lock (_lock) return _count; }
        }

        public CircularBuffer(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
            _buffer = new T[capacity];
        }

        public void Add(T item)
        {
            lock (_lock)
            {
                int index = (_start + _count) % _capacity;
                _buffer[index] = item;

                if (_count < _capacity)
                {
                    _count++;
                }
                else
                {
                    _start = (_start + 1) % _capacity;
                }
            }
        }

        public List<T> GetAll()
        {
            lock (_lock)
            {
                var result = new List<T>(_count);
                for (int i = 0; i < _count; i++)
                {
                    int index = (_start + i) % _capacity;
                    result.Add(_buffer[index]);
                }
                return result;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                Array.Clear(_buffer, 0, _buffer.Length);
                _start = 0;
                _count = 0;
            }
        }
    }
}
