using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.CGE.Helpers
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] buffer;
        private int start;
        private int end;
        private int currentSize;

        public CircularBuffer(int capacity)
        {
            if (capacity < 2) throw new ArgumentException("The buffer must have at least 2 items");

            buffer = new T[capacity];
            start = 0;
            end = 0;
        }

        public int Size => currentSize;
        public int Capacity => buffer.Length;
        public bool IsEmpty => currentSize == 0;
        public bool IsFull => currentSize == buffer.Length;

        public T this[int index]
        {
            get
            {
                if (index > currentSize) throw new IndexOutOfRangeException();
                int indexOnBuffer = getModularIndex(index);
                return buffer[indexOnBuffer];
            }
            set
            {
                if (index > currentSize) throw new IndexOutOfRangeException();
                int indexOnBuffer = getModularIndex(index);
                buffer[indexOnBuffer] = value;
            }
        }

        public void Add(T item)
        {
            lock (buffer)
            {
                buffer[end] = item;

                if (IsFull)
                {
                    modularIncrement(ref end); // change [IsFull] property
                    start = end;
                }
                else
                {
                    modularIncrement(ref end); // change [IsFull] property
                    currentSize++;
                }
            }
        }

        public T First()
        {
            if (IsEmpty) throw new IndexOutOfRangeException("The buffer is empty");
            return buffer[start];
        }
        public T Last()
        {
            if (IsEmpty) throw new IndexOutOfRangeException("The buffer is empty");
            return buffer[end];
        }

        private void modularIncrement(ref int value)
        {
            value++;
            if (value >= Capacity) value = 0;
        }
        private int getModularIndex(int externalIndex)
        {
            return (externalIndex + start) % Capacity;
        }

        private T[] toArray()
        {
            T[] arr = new T[currentSize];
            for (int i = 0; i < currentSize; i++)
            {
                arr[i] = this[i];
            }
            return arr;
        }
        public T[] ToArray()
        {
            return toArray();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var v in toArray()) // create a copy
            {
                yield return v;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return toArray().GetEnumerator();
        }
    }
}
