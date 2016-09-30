using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading;
using System.Diagnostics.Contracts;

namespace DataManager.Common
{
    public class ThreadMap4Items<TKey, TValue> where TValue:new()
    {
        readonly ReaderWriterLockSlim _gate;
        readonly IDictionary<TKey, Countable<TValue>> _map;
        readonly Action<TValue> _disposer;



        public ThreadMap4Items(int capacity=64, Action<TValue> valueDisposer=null)
        {
            _gate = new ReaderWriterLockSlim();
            _map = new Dictionary<TKey, Countable<TValue>>(capacity);
            _disposer = valueDisposer;
        }

        public void Add(TKey key, TValue value)
        {
            Contract.Requires(value != null);
            Contract.Requires(key != null);

            var item = new Countable<TValue>(value);

            _gate.EnterWriteLock();
            _map.Add(key, item);
            _gate.ExitWriteLock();
        }
        public void Delete(TKey key)
        {
            Contract.Requires(key != null);

            var obj = this[key];

            _gate.EnterWriteLock();
            _map.Remove(key);
            _gate.ExitWriteLock();

            DisposeObjects(new TValue[] { obj });
        }

        public bool Has(TKey key)
        {
            Contract.Requires(key != null);

            var ret = false;

            _gate.EnterReadLock();
             ret = _map.ContainsKey(key);
            _gate.ExitReadLock();

            return ret;
        }
        public TValue this[TKey key]
        {
            get
            {
                Contract.Requires(key != null);
                Countable<TValue> value = null; 

                _gate.EnterReadLock();
                if (_map.ContainsKey(key))
                    value = _map[key];
                _gate.ExitReadLock();

                value.Increment();
                return value.Item;
            }
        }

        public void Free(TKey key)
        {
            Contract.Requires(key != null);

            Countable<TValue> value=null;

            _gate.EnterReadLock();
            if (_map.ContainsKey(key))
                value = _map[key];
            _gate.ExitReadLock();

            value.Decrement();
        }
        public int ReferenceCount(TKey key)
        {
            Contract.Requires(key != null);

            var ret = 0;

            _gate.EnterReadLock();
            ret = _map[key].Count;
            _gate.ExitReadLock();

            return ret;
        }

#if DEBUG
        public override string ToString()
        {
            var ret = "";

            _gate.EnterReadLock();
            ret = string.Join(", ", _map.Keys);
            _gate.ExitReadLock();

            return ret;
        }
#endif

        public void Clear()
        {
            TValue[] objects;

            _gate.EnterWriteLock();
                objects = _map.Values.Select(x => x.Item).ToArray();
                _map.Clear();
            _gate.ExitWriteLock();

            DisposeObjects(objects);

        }

        private void DisposeObjects(TValue[] objects)
        {
            if (_disposer != null)
            {
                var errors = new List<Exception>();
                foreach (var it in objects)
                    try
                    {
                        _disposer(it);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                    }
                if (errors.Count > 0)
                    throw new AggregateException(errors.ToArray());
            }
        }

    }
}
