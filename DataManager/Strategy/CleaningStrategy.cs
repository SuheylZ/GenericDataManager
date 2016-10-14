using System;
using System.Collections.Generic;
using System.Linq;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;
using GenericDataManager.Providers;

namespace GenericDataManager.Strategies
{
    public class CleaningStrategyBase: 
        IDisposable 
    {
        protected readonly IContextMap _map;
        protected readonly ExecutionPolicy KPolicy;

        internal CleaningStrategyBase(IContextMap map, ExecutionPolicy policy)
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

            var keys = _map.Keys;       //Keys will change so get a snapshot before starting to remove and use this snapshot 
            foreach (var key in keys)
            {
                try
                {
                    var provider = _map.Remove(key) as ContextProviderBase;
                    if (provider.ConsumerCount > 0)
                        builder.Add(new Exception($"Provider for thread {key} has {provider.ConsumerCount} consumer(s)"));
                    provider.Dispose();
                }
                catch(Exception ex)
                {
                    builder.Add(ex);
                }
            }

            if (KPolicy.FinalDisposalBehaviour == ManagerDisposalStrategy.DisposeButThrowIfInUse && builder.HasErrors)
                throw builder.ToAggregateException();
        }
    }
}
