using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics.Contracts;
using System.Collections;

namespace GenericDataManager.Common
{
    public class ThreadMap4Items<TKey, TValue>: IEnumerable<TValue> where TValue:class
    {
        ReaderWriterLockSlim _gate;
        IDictionary<TKey, Countable<TValue>> _map;


        public ThreadMap4Items(int capacity=64)
        {
            _gate = new ReaderWriterLockSlim();
            _map = new Dictionary<TKey, Countable<TValue>>(capacity);
        }

        public TValue Add(TKey key, TValue value)
        {
            Contract.Requires(value != null);

            var item = new Countable<TValue>(value);

            _gate.EnterWriteLock();
            _map.Add(key, item);
            _gate.ExitWriteLock();

            return item.Item;
        }
        public TValue Delete(TKey key)
        {
            Contract.Ensures(Contract.Result<TValue>() != null);

            var ret = this[key];

            _gate.EnterWriteLock();
            _map.Remove(key);
            _gate.ExitWriteLock();

            return ret;
        }

        public bool Has(TKey key)
        {
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
                TValue ret = default(TValue);

                _gate.EnterReadLock();
                if (_map.ContainsKey(key))
                {
                    var item = _map[key];
                    item.Increment();
                    ret = item.Item;
                }
                _gate.ExitReadLock();

                return ret;
            }

        }
        public TValue this[int idx]
        {
            get
            {
                TValue ret = default(TValue);

                _gate.EnterReadLock();

                if (idx<_map.Count)
                {
                    var obj = _map.ElementAt(idx);
                    ret = obj.Value.Item;
                }
                _gate.ExitReadLock();
                return ret;
            }
        }
        public int Free(TKey key)
        {
            var ret = 0;

            _gate.EnterReadLock();
            _map[key].Decrement();
            ret = _map[key].Count;
            _gate.ExitReadLock();

            return ret;
        }
        public int ValueCount(TKey key)
        {
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

        public List<TValue> Clear()
        {
            var ret = new List<TValue>(_map.Count);

            _gate.EnterReadLock();
            ret = _map.Values.Select(x => x.Item).ToList();
            _gate.ExitReadLock();

            _gate.EnterWriteLock();
            _map.Clear();
            _gate.ExitWriteLock();

            return ret;
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var values = _map.Values.Select(x => x.Item).ToList();
            foreach(var it in values)
                yield return it;
        }
    }
}
