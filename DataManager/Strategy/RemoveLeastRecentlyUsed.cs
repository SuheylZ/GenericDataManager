using System;
using System.Collections.Generic;
using System.Linq;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;
using GenericDataManager.Providers;

namespace GenericDataManager.Strategies
{
    public class RemoveLeastRecentlyUsed :
        CleaningStrategyBase
    {
        readonly TimeSpan _minAge;
        Func<TimeSpan, bool> IsOlder => (age) => TimeSpan.Compare(_minAge, age) < 0;

        internal RemoveLeastRecentlyUsed(IContextMap map, ExecutionPolicy policy) : base(map, policy)
        {
            _minAge = TimeSpan.FromSeconds(10);
            _minAge = policy.MinimumAge;
        }

        protected internal override void Clean()
        {
            var values = _map.Values;
            var leastUsed = TimeSpan.Zero;

            var dispoablePairs = new List<ContextProviderThreadPair>();
            foreach (var pair in values)
            {
                var provider = pair.Provider as ContextProviderWithAge;
                if (provider != null)
                {
                    if (provider.ConsumerCount > 0)
                    {
                        if (IsOlder(provider.LastUsed))
                            dispoablePairs.Add(pair);
                    }
                    else
                        dispoablePairs.Add(pair);

                }
            }

            var keys = dispoablePairs.Select(x => x.Thread.ManagedThreadId).ToList();
            foreach (var key in keys)
            {
                var tmp = _map[key].Provider as ContextProviderWithAge;
                if (tmp != null && IsOlder(tmp.LastUsed))
                {
                    var disposable = _map.Remove(key);
                    if(disposable !=null)
                    disposable.Dispose();
                }

            }
        }
    }
}
