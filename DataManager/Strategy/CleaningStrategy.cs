using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;
using GenericDataManager.Providers;

namespace GenericDataManager.Strategies
{
    public class CleaningStrategy: IDisposable 
    {
        protected readonly IContextMap _map;
        protected readonly ExecutionPolicy KPolicy;
        //protected readonly System.Timers.Timer _timer;

        internal CleaningStrategy(IContextMap map, ExecutionPolicy policy)
        {
            KPolicy = policy;
            _map = map;

            //_timer = new System.Timers.Timer(KPolicy.HeartBeat.TotalMilliseconds);
            //_timer.Elapsed += (sender, args) =>
            // {
            //     _timer.Enabled = false;
            //     Clean();
            //     _timer.Enabled = true;
            // };
        }

        protected internal virtual void Clean()
        {
        }

        //void ICleaningStrategy.Start()
        //{
        //    //_timer.Start();
        //}

        //void ICleaningStrategy.Stop() {
        //    //_timer.Stop();
        //}

        void IDisposable.Dispose()
        {
            var builder = new AggregateExceptionBuilder("Error while disposing the DbContext objects");

            //_timer.Stop();
            var keys = _map.Keys;
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                var count = (_map[key].Provider as ContextProviderBase).ConsumerCount;
                if (count > 0)
                    builder.Add(new Exception($"Provider for thread {key} has {count} consumer(s)"));
                (_map.Remove(key) as IDisposable).Dispose();
            }
           // _timer.Dispose();

            if (KPolicy.FinalDisposalBehaviour == ManagerDisposalStrategy.DisposeButThrowIfInUse && builder.HasErrors)
                throw builder.ToAggregateException();
        }
    }

    public class RemoveUnused: CleaningStrategy
    {
        internal RemoveUnused(IContextMap map, ExecutionPolicy policy) : base(map, policy) { }

        protected internal override void Clean()
        {
           foreach(var key in _map.Keys)
            {
                var provider = _map[key].Provider as ContextProviderBase;
                if(provider.ConsumerCount == 0)
                {
                    _map.Remove(key);
                    provider.Dispose();
                }
            }
        }
    }

    public class RemoveOnlyWhenThreadIsDead: CleaningStrategy
    {
        internal RemoveOnlyWhenThreadIsDead(IContextMap map, ExecutionPolicy policy) : base(map, policy) { }

        protected internal override void Clean()
        {
            var keys = _map.Keys;
            foreach(var key in keys)
            {
                var pair = _map[key];
                if (!pair.Thread.IsAlive)
                {
                    _map.Remove(key);
                    var disposable = pair.Provider as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }

            }
        }
    }

    public class RemoveLeastRecentlyUsed: CleaningStrategy
    {
        readonly TimeSpan _minAge = TimeSpan.FromSeconds(10);

        internal RemoveLeastRecentlyUsed(IContextMap map, ExecutionPolicy policy) : base(map, policy)
        {
            _minAge = policy.MinimumAge;
        }

        protected internal override void Clean()
        {
            var values = _map.Values;
            var leastUsed = TimeSpan.Zero;

            var dispoablePairs = new List<ContextProviderThreadPair>();
            foreach(var pair in values)
            {
                var provider = pair.Provider as ContextProviderWithAge;
                if (provider != null)
                {
                    if (provider.ConsumerCount == 0)
                    {
                        if (TimeSpan.Compare(_minAge, provider.LastUsed) < 0)
                            dispoablePairs.Add(pair);
                    } 
                }
            }

            var keys = dispoablePairs.Select(x => x.Thread.ManagedThreadId).ToList();
            foreach(var key in keys)
            {
                var tmp = _map[key].Provider as ContextProviderWithAge;
                if (tmp != null && tmp.ConsumerCount == 0 && TimeSpan.Compare(_minAge, tmp.LastUsed) < 0)
                    _map.Remove(key);
            }
        }
    }
}
