﻿using System;
using UnityEngine;

namespace Framework.Pool
{
    /// <summary>
    /// 对象池组件。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ObjectPoolComponent : FrameworkComponent
    {
        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            InstanceObjectManager.Instance.CreateInstancePool();
        }

        /// <summary>
        /// 获取对象池数量。
        /// </summary>
        public int Count
        {
            get
            {
                return ObjectPoolManager.Instance.Count;
            }
        }

        /// <summary>
        /// 检查是否存在对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>是否存在对象池。</returns>
        public bool HasObjectPool<T>() where T : ObjectBase
        {
            return ObjectPoolManager.Instance.HasObjectPool<T>();
        }

        /// <summary>
        /// 检查是否存在对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <returns>是否存在对象池。</returns>
        public bool HasObjectPool(Type objectType)
        {
            return ObjectPoolManager.Instance.HasObjectPool(objectType);
        }

        /// <summary>
        /// 检查是否存在对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <returns>是否存在对象池。</returns>
        public bool HasObjectPool<T>(string name) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.HasObjectPool<T>(name);
        }

        /// <summary>
        /// 检查是否存在对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <returns>是否存在对象池。</returns>
        public bool HasObjectPool(Type objectType, string name)
        {
            return ObjectPoolManager.Instance.HasObjectPool(objectType, name);
        }

        /// <summary>
        /// 获取对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>要获取的对象池。</returns>
        public ObjectPool<T> GetObjectPool<T>() where T : ObjectBase
        {
            return ObjectPoolManager.Instance.GetObjectPool<T>();
        }

        /// <summary>
        /// 获取对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <returns>要获取的对象池。</returns>
        public ObjectPoolBase GetObjectPool(Type objectType)
        {
            return ObjectPoolManager.Instance.GetObjectPool(objectType);
        }

        /// <summary>
        /// 获取对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <returns>要获取的对象池。</returns>
        public ObjectPool<T> GetObjectPool<T>(string name) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.GetObjectPool<T>(name);
        }

        /// <summary>
        /// 获取对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <returns>要获取的对象池。</returns>
        public ObjectPoolBase GetObjectPool(Type objectType, string name)
        {
            return ObjectPoolManager.Instance.GetObjectPool(objectType, name);
        }

        /// <summary>
        /// 获取所有对象池。
        /// </summary>
        public ObjectPoolBase[] GetAllObjectPools()
        {
            return ObjectPoolManager.Instance.GetAllObjectPools();
        }

        /// <summary>
        /// 获取所有对象池。
        /// </summary>
        /// <param name="sort">是否根据对象池的优先级排序。</param>
        /// <returns>所有对象池。</returns>
        public ObjectPoolBase[] GetAllObjectPools(bool sort)
        {
            return ObjectPoolManager.Instance.GetAllObjectPools(sort);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>() where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>();
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(string name) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(name);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, name);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="capacity">对象池的容量。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(capacity);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, capacity);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(float expireTime) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(expireTime);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, float expireTime)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, expireTime);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(name, capacity);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, name, capacity);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float expireTime) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(name, expireTime);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, float expireTime)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, name, expireTime);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, float expireTime) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(capacity, expireTime);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, float expireTime)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, capacity, expireTime);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(capacity, priority);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, int priority)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, capacity, priority);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(float expireTime, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(expireTime, priority);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, float expireTime, int priority)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, expireTime, priority);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, float expireTime) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(name, capacity, expireTime);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, float expireTime)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, name, capacity, expireTime);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(name, capacity, priority);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, int priority)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, name, capacity, priority);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float expireTime, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(name, expireTime, priority);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, float expireTime, int priority)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, name, expireTime, priority);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, float expireTime, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(capacity, expireTime, priority);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, float expireTime, int priority)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, capacity, expireTime, priority);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, float expireTime, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool<T>(name, capacity, expireTime, priority);
        }

        /// <summary>
        /// 创建允许单次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, float expireTime, int priority)
        {
            return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(objectType, name, capacity, expireTime, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>() where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>();
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(string name) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(name);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, name);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="capacity">对象池的容量。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(capacity);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, capacity);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(float expireTime) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(expireTime);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, float expireTime)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, expireTime);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(name, capacity);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, name, capacity);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, float expireTime) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(name, expireTime);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, float expireTime)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, name, expireTime);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, float expireTime) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(capacity, expireTime);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity, float expireTime)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, capacity, expireTime);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(capacity, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity, int priority)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, capacity, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(float expireTime, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(expireTime, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, float expireTime, int priority)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, expireTime, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity, float expireTime) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(name, capacity, expireTime);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity, float expireTime)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, name, capacity, expireTime);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(name, capacity, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity, int priority)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, name, capacity, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, float expireTime, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(name, expireTime, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, float expireTime, int priority)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, name, expireTime, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, float expireTime, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(capacity, expireTime, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity, float expireTime, int priority)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, capacity, expireTime, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity, float expireTime, int priority) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool<T>(name, capacity, expireTime, priority);
        }

        /// <summary>
        /// 创建允许多次获取的对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">对象池名称。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity, float expireTime, int priority)
        {
            return ObjectPoolManager.Instance.CreateMultiSpawnObjectPool(objectType, name, capacity, expireTime, priority);
        }

        /// <summary>
        /// 销毁对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>是否销毁对象池成功。</returns>
        public bool DestroyObjectPool<T>() where T : ObjectBase
        {
            return ObjectPoolManager.Instance.DestroyObjectPool<T>();
        }

        /// <summary>
        /// 销毁对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <returns>是否销毁对象池成功。</returns>
        public bool DestroyObjectPool(Type objectType)
        {
            return ObjectPoolManager.Instance.DestroyObjectPool(objectType);
        }

        /// <summary>
        /// 销毁对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">要销毁的对象池名称。</param>
        /// <returns>是否销毁对象池成功。</returns>
        public bool DestroyObjectPool<T>(string name) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.DestroyObjectPool<T>(name);
        }

        /// <summary>
        /// 销毁对象池。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="name">要销毁的对象池名称。</param>
        /// <returns>是否销毁对象池成功。</returns>
        public bool DestroyObjectPool(Type objectType, string name)
        {
            return ObjectPoolManager.Instance.DestroyObjectPool(objectType, name);
        }

        /// <summary>
        /// 销毁对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="objectPool">要销毁的对象池。</param>
        /// <returns>是否销毁对象池成功。</returns>
        public bool DestroyObjectPool<T>(ObjectPool<T> objectPool) where T : ObjectBase
        {
            return ObjectPoolManager.Instance.DestroyObjectPool(objectPool);
        }

        /// <summary>
        /// 销毁对象池。
        /// </summary>
        /// <param name="objectPool">要销毁的对象池。</param>
        /// <returns>是否销毁对象池成功。</returns>
        public bool DestroyObjectPool(ObjectPoolBase objectPool)
        {
            return ObjectPoolManager.Instance.DestroyObjectPool(objectPool);
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        public void Release()
        {
            Debug.Log("Object pool release...");
            ObjectPoolManager.Instance.Release();
        }

        /// <summary>
        /// 释放对象池中的所有未使用对象。
        /// </summary>
        public void ReleaseAllUnused()
        {
            Debug.Log("Object pool release all unused...");
            ObjectPoolManager.Instance.ReleaseAllUnused();
        }
    }
}
