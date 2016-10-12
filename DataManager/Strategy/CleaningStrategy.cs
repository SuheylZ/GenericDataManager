using System;
using System.Collections.Generic;
using System.Linq;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;
using GenericDataManager.Providers;

namespace GenericDataManager.Strategies
{
    public class CleaningStrategy: 
        IDisposable 
    {
        protected readonly IContextMap _map;
        protected readonly ExecutionPolicy KPolicy;

        internal CleaningStrategy(IContextMap map, ExecutionPolicy policy)
        {
            KPolicy = policy;
            _map = map;
        }

        protected internal virtual void Clean()
        {
        }

        void IDisposable.Dispose()
        {
            var builder = new AggregateExceptionBuilder("Error while disposing the DbContext objects");

            var keys = _map.Keys;
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                var count = (_map[key].Provider as ContextProviderBase).ConsumerCount;
                if (count > 0)
                    builder.Add(new Exception($"Provider for thread {key} has {count} consumer(s)"));
                var item = _map.Remove(key);
                if (item != null)
                    item.Dispose();
            }

            if (KPolicy.FinalDisposalBehaviour == ManagerDisposalStrategy.DisposeButThrowIfInUse && builder.HasErrors)
                throw builder.ToAggregateException();
        }
    }
}
