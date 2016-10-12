using System;
using System.Diagnostics.Contracts;
using System.Threading;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;
using GenericDataManager.Providers;
using GenericDataManager.Strategies;

namespace GenericDataManager
{
    /// <summary>
    /// DataManager keeps track of all EntityOperations and provides a single instantiation of EntityOperation of each particular entity.
    /// </summary>
    public class DataManagerWithPolicy: IDataRepositoryProvider
    {
        readonly static IContextMap _map = new ContextMap(Konstants.EstimatedThreads);

        readonly ConnectionParameters KConnectionParameters;
        readonly ExecutionPolicy KPolicy;

        readonly ICleaningStrategy _cleaner;
        readonly CreationStrategy _creator;

        /// <summary>
        /// Establishes the connection and initializes internal structures
        /// </summary>
        /// <param name="arg">Settings for the connection, model etc</param>
        /// <param name="policy">Policy for this datamanager</param>
        public DataManagerWithPolicy(ConnectionParameters arg, ExecutionPolicy policy)
        {
            Contract.Requires(!string.IsNullOrEmpty(arg.connection));
            Contract.Requires(!string.IsNullOrEmpty(arg.modelResource));

            KConnectionParameters = arg;
            KPolicy = policy;

            _creator = new CreationStrategy(KConnectionParameters, KPolicy);


            switch (KPolicy.PeriodicDisposalStrategy)
            {
                case Strategy.DisposeWithThread:
                    _cleaner = new RemoveOnlyWhenThreadIsDead(_map, KPolicy);
                    break;
                case Strategy.DisposeWhenNotInUse:
                    _cleaner = new RemoveUnused(_map, KPolicy);
                    break;
                case Strategy.DisposeLeastRecentlyUsed:
                    _cleaner = new RemoveLeastRecentlyUsed(_map, KPolicy);
                    break;
                default:
                    this._cleaner = new CleaningStrategy(_map, KPolicy);
                    break;
            }
            _cleaner.Start();

        }

        public IDataRepository Repository
        {
            get
            {
               var thread = Thread.CurrentThread;

                if (!_map.Has(thread))
                   _map.Add(thread, _creator.Create(thread));

                var provider = _map[thread.ManagedThreadId].Provider;
                return provider as IDataRepository;
            }

        }

        public void Dispose()
        {
            _cleaner.Dispose();

            //AggregateExceptionBuilder builder = new AggregateExceptionBuilder("Error while disposing the DataManager");
            //_cleaner.Stop();

            //var keys = _map.Keys;
            //var retries = 3;
            //for(var i=0;i<keys.Length;i++)
            //{
            //    var key = keys[i];
            //    var count = (_map[key].Provider as ContextProviderBase).ConsumerCount;
            //    var canRemove = false;

            //    if (count > 0)
            //    {
            //        switch (KPolicy.ManagerDisposeStrategy)
            //        {
            //            case ManagerDisposalStrategy.Default:
            //            case ManagerDisposalStrategy.DisposeButThrowIfInUse:
            //                builder.Add(new Exception($"Provider for thread {key} has {count} consumer(s)"));
            //                canRemove = true;
            //                break;

            //            case ManagerDisposalStrategy.FailIfNotDisposed:
            //                throw new Exception($"Context on thread {key} is still in use by {count} repositories");

            //            case ManagerDisposalStrategy.RetryUntilDisposedOrFail:
            //                Thread.Sleep(Convert.ToInt32(KPolicy.DisposalWait.TotalMilliseconds));
            //                i--; //Retry
            //                retries--;
            //                if (retries == 0)
            //                    throw new Exception($"Context on thread {key} is still in use by {count} repositories");
            //                break;

            //            case ManagerDisposalStrategy.DisposeSilentlyEvenIfInUse:
            //                canRemove = true;
            //                break;
            //        }
            //    }
            //    else
            //        canRemove = true;

            //    if (canRemove) {
            //        (_map.Remove(key) as IDisposable).Dispose();
            //        retries = KPolicy.RetryCount;
            //    }

            //}

            //if (builder.HasErrors)
            //    throw builder.ToAggregateException();
        }

#if DEBUG
        public override string ToString()
        {
            return $"_map: {_map.ToString()}";
        }
#endif
    }
}
