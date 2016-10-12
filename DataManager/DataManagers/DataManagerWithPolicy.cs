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

        readonly ICleaner _cleaner;
        readonly ContextProviderCreator _creator;

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

            _creator = new ContextProviderCreator(KConnectionParameters, KPolicy);

            switch (KPolicy.PeriodicDisposalStrategy)
            {
                case Strategy.DisposeWithThread:
                    _cleaner = new Cleaner<RemoveOnlyWhenThreadIsDead>(_map, KPolicy);
                    break;
                case Strategy.DisposeWhenNotInUse:
                    _cleaner = new Cleaner<RemoveUnused>(_map, KPolicy);
                    break;
                case Strategy.DisposeLeastRecentlyUsed:
                    _cleaner = new Cleaner<RemoveLeastRecentlyUsed>(_map, KPolicy);
                    break;
                default:
                    this._cleaner = new Cleaner<CleaningStrategy>(_map, KPolicy);
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
        }


#if DEBUG
        public override string ToString()
        {
            return $"_map: {_map.ToString()}";
        }
#endif
    }
}
