using System;
using System.Data;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;

namespace GenericDataManager.Consumers
{
    public class ContextConsumerBase : IContextConsumer
    {
        protected IContextProvider _provider;
        private string _key;

        protected static readonly Tuple<int, int, int, int> EntityFrameworkVersion = Konstants.GetEntityFrameworkVersion();

        internal ContextConsumerBase(IContextProvider arg)
        {
            _provider = arg;
        }

        void IContextConsumer.Init(IContextProvider arg, string key)
        {
            _provider = arg;
            _key = key;
        }

        protected void Save(bool useTransaction = false)
        {
            if (EntityFrameworkVersion.Item1 <= 5)
                EnsureConnectionOpen();


            try
            {
                _provider.DataContext.SaveChanges();
            }
            catch (Exception ex)
            {

                throw;
            }

            if (EntityFrameworkVersion.Item1 <= 5)
                EnsureConnectionOpen();
        }

        protected void EnsureConnectionOpen()
        {
                if (_provider.ObjectContext.Connection.State != ConnectionState.Open)
                    _provider.ObjectContext.Connection.Open();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                   _provider.Release(this, _key);
                    //_provider = null;   -- not required as cleaner will take care of the providers
                }
                disposedValue = true;
            }
        }

        ~ContextConsumerBase()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion



    }
}
