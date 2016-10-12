using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GenericDataManager.Interfaces;

namespace GenericDataManager.Common
{
    public class ContextProviderThreadPair
    {
        readonly Thread _thread;
        readonly IContextProvider _provider;
 
        internal ContextProviderThreadPair(Thread thread, IContextProvider provider)
        {
            _thread = thread;
            _provider = provider;
        }

        public Thread Thread => _thread;
        public IContextProvider Provider => _provider;
    }

    internal class ContextMap : IContextMap
    {
        ConcurrentDictionary<int, ContextProviderThreadPair> _map;
        //ReaderWriterLockSlim _lock;

        internal ContextMap(int estimatedThreadCount)
        {
            _map = new ConcurrentDictionary<int, ContextProviderThreadPair>(estimatedThreadCount, estimatedThreadCount);
            //_lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        void IContextMap.Add(Thread th, IContextProvider provider)
        {
            var tid = th.ManagedThreadId;
            if (!_map.ContainsKey(tid))
                _map.TryAdd(tid, new ContextProviderThreadPair(th, provider));
        }
        IContextProvider IContextMap.Remove(int id)
        {
            IContextProvider ret = null;


            if (_map.ContainsKey(id))
            {
                ContextProviderThreadPair tmp = null;
                _map.TryRemove(id, out tmp);
                if (tmp != null)
                    ret = tmp.Provider;
            }
            return ret;
        }
        ContextProviderThreadPair IContextMap.this[int tid]
        {
            get
            {
                ContextProviderThreadPair ret = null;
                _map.TryGetValue(tid, out ret);
                return ret;
            }
        }
        ContextProviderThreadPair[] IContextMap.Clear()
        {
            var values = new List<ContextProviderThreadPair>(_map.Count);

            var keys = _map.Keys;
            foreach(var it in keys)
            {
                ContextProviderThreadPair tmp;
                _map.TryRemove(it, out tmp);
                if (tmp != null)
                    values.Add(tmp);
            }
            return values.ToArray();
        }

        int[] IContextMap.Keys => _map.Keys.ToArray();
        ContextProviderThreadPair[] IContextMap.Values => _map.Values.ToArray();
        bool IContextMap.Has(Thread thread) => _map.ContainsKey(thread.ManagedThreadId);

    }
}
