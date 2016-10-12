using System;
using System.Collections.Generic;
using System.Linq;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;
using GenericDataManager.Providers;

namespace GenericDataManager.Strategies
{
    public class RemoveUnused :
        CleaningStrategy
    {
        internal RemoveUnused(IContextMap map, ExecutionPolicy policy) : base(map, policy) { }

        protected internal override void Clean()
        {
            foreach (var key in _map.Keys)
            {
                var provider = _map[key].Provider as ContextProviderBase;
                if (provider.ConsumerCount == 0)
                {
                    _map.Remove(key);
                    provider.Dispose();
                }
            }
        }
    }

}
