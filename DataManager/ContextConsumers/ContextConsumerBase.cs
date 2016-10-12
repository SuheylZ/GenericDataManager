using System;
using System.Data;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;

namespace GenericDataManager.Consumers
{
    public class ContextConsumerBase : IContextConsumer, IDisposable
    {
        protected IContextProvider _provider;
        private string _key;
        protected static readonly Tuple<int, int, int, int> EntityFrameworkVersion = Konstants.GetEntityFrameworkVersion();

        internal ContextConsumerBase(IContextProvider arg)
        {
            _provider = arg;
        }
        void IDisposable.Dispose()
        {
            _provider.Release(this, _key);
        }

        void IContextConsumer.Init(IContextProvider arg, string key)
        {
            _provider = arg;
            _key = key;
        }
        void IContextConsumer.Cleanup()
        {
            InnerCleanup();
        }


        protected void Save(bool useTransaction = false)
        {
            if (EntityFrameworkVersion.Item1 <= 5)
                EnsureConnectionOpen();

            //if (false)
            //{
            //    using (var trx = _provider.DataContext.Database.Connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            //    {
            //        _provider.DataContext.SaveChanges();
            //        trx.Commit();
            //    }
            //}
            //else
                _provider.DataContext.SaveChanges();

            if (EntityFrameworkVersion.Item1 <= 5)
                EnsureConnectionOpen();
        }

        protected void EnsureConnectionOpen()
        {
                if (_provider.ObjectContext.Connection.State != ConnectionState.Open)
                    _provider.ObjectContext.Connection.Open();
        }

        protected virtual void InnerCleanup()
        {}

    }
}
