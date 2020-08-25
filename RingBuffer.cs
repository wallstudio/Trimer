
using System;
using System.Collections;
using System.Collections.Generic;

namespace Trimer
{
    public class RingBuffer<T> : IEnumerable<T>
    {
        readonly T[] m_Buffer;
        int m_Postion = 0;
        
        public int Length => m_Buffer.Length;
        public T Current => m_Buffer[m_Postion];

        public RingBuffer(int count) : this(count, null) {}
        public RingBuffer(int count, Func<T> factory)
        {
            factory ??= () => default(T);
            m_Buffer = new T[count];
            for (int i = 0; i < m_Buffer.Length; i++)
            {
                m_Buffer[i] = factory();
            }
        }

        public void Add(T item)
        {
            m_Postion = (m_Postion + 1) % m_Buffer.Length;
            m_Buffer[m_Postion] = item;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            var start = m_Postion + 1;
            for (int i = 0; i < m_Buffer.Length; i++)
            {
                var index = (start + i) % m_Buffer.Length;
                yield return m_Buffer[index];
            }
        }

    }
}