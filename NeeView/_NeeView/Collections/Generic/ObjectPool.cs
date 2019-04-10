﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView.Collections.Generic
{
    /// <summary>
    /// オブジェクトのリサイクル
    /// </summary>
    public class ObjectPool<T> where T : new()
    {
        private Queue<T> _pool = new Queue<T>();
        private object _lock = new object();
        private int _count;

        public ObjectPool()
        {
        }

        public ObjectPool(int size)
        {
            for (int i = 0; i < size; ++i)
            {
                _count++;
                _pool.Enqueue(new T());
            }
        }

        public T Allocate()
        {
            lock (_lock)
            {
                if (_pool.Any())
                {
                    ////Debug.WriteLine($"{typeof(T)} Pool: Recycle {_pool.Count}");
                    return _pool.Dequeue();
                }
                else
                {
                    _count++;
                    ////Debug.WriteLine($"{typeof(T)} Pool: New #{_count}");
                    return new T();
                }
            }
        }

        public void Release(T element)
        {
            lock (_lock)
            {
                if (!_pool.Contains(element))
                {
                    _pool.Enqueue(element);
                    ////Debug.WriteLine($"{typeof(T)} Pool: Release: {_pool.Count}");
                }
            }
        }
    }

}
