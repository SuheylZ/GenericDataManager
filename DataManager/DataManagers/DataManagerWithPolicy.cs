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
                    this._cleaner = new Cleaner<CleaningStrategyBase>(_map, KPolicy);
                    break;
            }

            _cleaner.Start();
        }

        IDataRepository IDataRepositoryProvider.Repository
        {
            get
            {
               var thread = Thread.CurrentThread;

                if (!_map.Has(thread))
                   _map.Add(thread, _creator.Create(thread));

                return _map[thread.ManagedThreadId].Provider as IDataRepository;
            }
        }


#if DEBUG
        public override string ToString()
        {
            return $"_map: {_map.ToString()}";
        }
#endif


        #region IDisposable Support
        private bool disposedValue = false; 

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                   _cleaner.Dispose(); 
                }
                disposedValue = true;
            }
        }

        ~DataManagerWithPolicy()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
