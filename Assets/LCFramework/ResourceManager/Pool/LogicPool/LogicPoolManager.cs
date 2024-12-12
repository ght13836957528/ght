using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Pool
{
    // -------------------------------------------------------------------------------------------
    // @说      明: 业务逻辑对象池管理器
    // @作      者: zhoumingfeng
    // @版  本  号: V1.00
    // @创建时间: 2024.05.29
    // -------------------------------------------------------------------------------------------
    public class LogicPoolManager : GameBaseSingletonModule<LogicPoolManager>
    {
        private Dictionary<Type, Queue<object>> _objectPool = new Dictionary<Type, Queue<object>>();
        private Dictionary<Type, int> _poolCapacity = new Dictionary<Type, int>();

        // 池中默认2个对象;
        private int _default_count = 2;

        // 池子储存对象的容量;
        private int _default_capacity = 100;

        /// <summary>
        /// 从池中取出对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Spawn<T>() where T : BaseLogicData, new()
        {
            lock (_objectPool)
            {
                var key = typeof(T);

                Queue<object> queue = null;
                _objectPool.TryGetValue(key, out queue);

                if (queue == null)
                {
                    queue = new Queue<object>();
                    _objectPool.Add(key, queue);
                }

                if (queue.Count > _default_count)
                {
                    T ret = (T)queue.Dequeue();
                    ret.Clear();
                    return ret;
                }
                else
                {
                    return new T();
                }
            }
        }

        /// <summary>
        /// 对象回收
        /// </summary>
        /// <param name="obj">回池的对象</param>
        public void Recycle(object obj)
        {
            if (null == obj)
                return;

            var key = obj.GetType();

            Queue<object> queue = null;
            _objectPool.TryGetValue(key, out queue);

            if (null == queue)
            {
                queue = new Queue<object>();
                _objectPool[key] = queue;
            }

            int capacity = _default_capacity;
            if (_poolCapacity.ContainsKey(key))
                capacity = _poolCapacity[key];

            // 池中超过容量,那么清理;
            if (queue.Count > capacity)
            {
                queue.Clear();

                Debug.LogWarning($"{obj.GetType().ToString()}类型的对象池容量({capacity})已满,池子被清空！");
            }

            if (!queue.Contains(obj))
                queue.Enqueue(obj);
        }

        /// <summary>
        /// 设置某个类型池的容量
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="capacity">容量</param>
        public void SetCapacity<T>(int capacity) where T : BaseLogicData, new()
        {
            if (capacity <= 0)
                return;

            var key = typeof(T);

            if (_poolCapacity.ContainsKey(key))
            {
                _poolCapacity[key] = capacity;
            }
            else
            {
                _poolCapacity.Add(key, capacity);
            }

           
        }

        public int GetCapacity(Type type)
        {
            if (_poolCapacity.ContainsKey(type))
                return _poolCapacity[type];

            return _default_capacity;
        }

        public int GetPoolsCount()
        {
            return this._objectPool.Count;
        }

        public Dictionary<Type, Queue<object>> GetAllPools()
        {
            return this._objectPool;
        }

        public override void Shutdown() 
        {
            _poolCapacity.Clear();

            foreach (var per in _objectPool)
                per.Value.Clear();

            _objectPool.Clear();
        }
    }
}