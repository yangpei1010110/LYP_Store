using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace LYP_Utils.LYP_Store
{
    /// <summary>
    ///     thread safe zero alloc query store in memory
    ///     1. add or update data by key (alloc)
    ///     2. remove data by handle cache (non alloc)
    ///     3. try query data by handle cache (non alloc)
    ///     4. try update data by handle cache (non alloc)
    /// </summary>
    public class LStore<TKey, TValue> : IReadOnlyCollection<TValue>
    {
        public  TimeSpan             _lockTimeout = TimeSpan.FromSeconds(1);
        private ReaderWriterLockSlim _lock        = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private int[]    HandleVersionArray = Array.Empty<int>();
        private TValue[] ValueArray         = Array.Empty<TValue>();

        private Dictionary<TKey, int> _keyToHandle = new Dictionary<TKey, int>();

        private LHandlePool _handlePool = new LHandlePool();

        /// <summary>
        ///     resize array
        /// </summary>
        private void ResizeNewArray(int needSize)
        {
            if (ValueArray.Length >= needSize)
            {
                return;
            }

            const int minIncreamentSize = 128;

            int newSize = Math.Max(needSize, ValueArray.Length + minIncreamentSize);
            Array.Resize(ref HandleVersionArray, newSize);
            Array.Resize(ref ValueArray, newSize);
        }

        /// <summary>
        ///     check handle is valid
        /// </summary>
        private bool ValidateHandle(in LHandle<TKey> handle)
        {
            if (handle.Handle < 0 || handle.Handle >= ValueArray.Length)
            {
                return false;
            }

            if (handle.HandleVersion != HandleVersionArray[handle.Handle])
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     clear all data
        /// </summary>
        public void Clear()
        {
            if (!_lock.TryEnterWriteLock(_lockTimeout))
            {
                return;
            }

            try
            {
                HandleVersionArray = Array.Empty<int>();
                ValueArray = Array.Empty<TValue>();
                _keyToHandle.Clear();
                _handlePool.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     cache the handle of the key
        /// </summary>
        public bool TryHandle(in TKey key, out LHandle<TKey> handle)
        {
            handle = default(LHandle<TKey>);
            if (!_lock.TryEnterReadLock(_lockTimeout))
            {
                return false;
            }

            try
            {
                if (!_keyToHandle.TryGetValue(key, out int handleIndex))
                {
                    return false;
                }

                handle = new LHandle<TKey>(handleIndex, HandleVersionArray[handleIndex], key);
                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }


        /// <summary>
        ///     add or update data by key
        /// </summary>
        public bool TryAddOrUpdate(in TKey key, in TValue value, out LHandle<TKey> handle)
        {
            handle = default(LHandle<TKey>);
            if (!_lock.TryEnterWriteLock(_lockTimeout))
            {
                return false;
            }

            try
            {
                if (TryHandle(key, out handle))
                {
                    return TryUpdate(handle, value);
                }
                else
                {
                    int index = _handlePool.RentOrAddHandle();
                    ResizeNewArray(index + 1);
                    HandleVersionArray[index] += 1;
                    ValueArray[index] = value;
                    _keyToHandle.Add(key, index);
                    handle = new LHandle<TKey>(index, HandleVersionArray[index], key);
                    return true;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     remove data by handle cache
        /// </summary>
        public bool TryRemove(in LHandle<TKey> handle)
        {
            if (!_lock.TryEnterWriteLock(_lockTimeout))
            {
                return false;
            }

            try
            {
                if (!ValidateHandle(handle))
                {
                    return false;
                }

                HandleVersionArray[handle.Handle] += 1;
                _keyToHandle.Remove(handle.Key);
                _handlePool.ReleaseHandle(handle.Handle);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     try query data by handle cache
        /// </summary>
        public bool TryQuery(in LHandle<TKey> handle, out TValue value)
        {
            value = default(TValue);
            if (!_lock.TryEnterReadLock(_lockTimeout))
            {
                return false;
            }

            try
            {
                if (!ValidateHandle(handle))
                {
                    return false;
                }

                value = ValueArray[handle.Handle];
                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        ///     try update data by handle cache
        /// </summary>
        public bool TryUpdate(in LHandle<TKey> handle, in TValue newValue)
        {
            if (!_lock.TryEnterWriteLock(_lockTimeout))
            {
                return false;
            }

            try
            {
                if (!ValidateHandle(handle))
                {
                    return false;
                }

                ValueArray[handle.Handle] = newValue;
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     inner handle pool
        /// </summary>
        internal class LHandlePool
        {
            private SpinLock _lock;

            private int _handleCounter;
            private int HandleCounter => Interlocked.Increment(ref _handleCounter);

            private Stack<int>   HandlePool  = new Stack<int>();
            private HashSet<int> ExistHandle = new HashSet<int>();

            internal void Clear()
            {
                bool lockTaken = false;
                try
                {
                    _lock.Enter(ref lockTaken);
                    HandlePool.Clear();
                    ExistHandle.Clear();
                    _handleCounter = 0;
                }
                finally
                {
                    if (lockTaken)
                    {
                        _lock.Exit();
                    }
                }
            }

            internal int RentOrAddHandle()
            {
                bool lockTaken = false;
                try
                {
                    _lock.Enter(ref lockTaken);
                    int handle = HandlePool.Count > 0 ? HandlePool.Pop() : HandleCounter;
                    ExistHandle.Remove(handle);
                    return handle;
                }
                finally
                {
                    if (lockTaken)
                    {
                        _lock.Exit();
                    }
                }
            }

            internal void ReleaseHandle(int handle)
            {
                bool lockTaken = false;
                try
                {
                    _lock.Enter(ref lockTaken);
                    if (ExistHandle.Contains(handle))
                    {
                        return;
                    }

                    HandlePool.Push(handle);
                    ExistHandle.Add(handle);
                }
                finally
                {
                    if (lockTaken)
                    {
                        _lock.Exit();
                    }
                }
            }
        }

        #region IReadOnlyCollection

        /// <summary>
        ///     not thread safe
        /// </summary>
        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (int valueIndex in _keyToHandle.Values)
            {
                yield return ValueArray[valueIndex];
            }
        }

        /// <summary>
        ///     not thread safe
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _keyToHandle.Count;

        #endregion
    }
}