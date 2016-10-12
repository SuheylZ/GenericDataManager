using System;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;

namespace GenericDataManager.Strategies
{
    public class RemoveOnlyWhenThreadIsDead :
        CleaningStrategy
    {
        internal RemoveOnlyWhenThreadIsDead(IContextMap map, ExecutionPolicy policy) : base(map, policy) { }

        protected internal override void Clean()
        {
            var keys = _map.Keys;
            foreach (var key in keys)
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
}
