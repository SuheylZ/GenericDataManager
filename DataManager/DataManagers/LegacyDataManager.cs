using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Diagnostics.Contracts;
using System.Threading;
using GenericDataManager.Common;
using GenericDataManager.Interfaces;
using GenericDataManager.Providers;

namespace GenericDataManager
{

    /// <summary>
    /// DataManager keeps track of all EntityOperations and provides a single instantiation of EntityOperation of each particular entity.
    /// </summary>
    public class DataManager :
        IEntityGateway,
        IDataManager
    {
        ConnectionParameters KConnectionParameters;
        static ThreadMap4Items<int, object> _map;

        /// <summary>
        /// Establishes the connection and initializes internal structures
        /// </summary>
        /// <param name="connectionStr">basic connection string</param>
        /// <param name="model">the name of the model without the extensions</param>
        /// <param name="provider">provider name, default is System.Data.SqlClient</param>
        /// <param name="maxWait">The maximum duration for after which the operation must fail if lock could not be acquired.</param>
        public DataManager(ConnectionParameters arg)
        {
            Contract.Requires(!string.IsNullOrEmpty(arg.connection));
            Contract.Requires(!string.IsNullOrEmpty(arg.modelResource));

            KConnectionParameters = arg;
            _map = new ThreadMap4Items<int, object>(16);
            _map.Add(Thread.CurrentThread.ManagedThreadId, CreateDbContext());
        }

        void IEntityGateway.Release(int id)
        {
            if (_map.Has(id))
            {
                var count = _map.Free(id);
            }
        }

        /// <summary>
        /// Retrieves the EntityOperation for a particular entity type. Only a single instance is instantiated
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IEntityReaderWriter<TEntity> IDataManager.Get<TEntity>() {
            Contract.Ensures(Contract.Result<IEntityReaderWriter<TEntity>>() != null);

            var clientThreadId = Thread.CurrentThread.ManagedThreadId;
            var provider = (this as IEntityGateway);
            

            IEntityReaderWriter<TEntity> ret = null;
            if (!_map.Has(clientThreadId))
                _map.Add(clientThreadId, CreateDbContext());

            ret = new LegacyProvider<TEntity>();
            (ret as Interfaces.IEntityGatewayClient).Register(this as IEntityGateway, clientThreadId);

            return ret;
        }

        void IDisposable.Dispose()
        {
            var contexts = _map.Clear();
            var builder = new AggregateExceptionBuilder("Errors encountered while disposing DbContext objects");

            foreach(var it in contexts)
            {
                try
                {
                    (it as IDisposable).Dispose();
                }
                catch(Exception ex)
                {
                    builder.Add(ex);
                }
            }

            if (builder.HasErrors)
                throw builder.ToAggregateException();
        }

        DbSet<TEntity> IEntityGateway.Set<TEntity>()
        {
            return (this as IEntityGateway).Context.Set<TEntity>();
        }

        ObjectContext IEntityGateway.ObjectContext
        {
            get
            {
                ObjectContext ret = null;
                var adapter = (this as IEntityGateway).Context as IObjectContextAdapter;
                if (adapter != null)
                    ret = adapter.ObjectContext;
                return ret;
            }
        }

        DbContext IEntityGateway.Context
        {
            get
            {
                var value = _map[Thread.CurrentThread.ManagedThreadId] as DbContext;

                return value;
            }
        }

        private DbContext CreateDbContext()
        {
            Contract.Ensures(Contract.Result<DbContext>() != null);

            DbContext ret = null;
            var builder = new EntityConnectionStringBuilder
            {
                ProviderConnectionString = KConnectionParameters.connection,
                Metadata = KConnectionParameters.modelResource,
                Provider = KConnectionParameters.provider
            };
            ret = new DbContext(builder.ConnectionString);

            if (ret.Database.Connection.State != System.Data.ConnectionState.Open)
                ret.Database.Connection.Open();

            return ret;
        }

        void IEntityGateway.Save()
        {
            try
            {
                (this as IEntityGateway).Context.SaveChanges();
            }
            catch (Exception ex)
            {
                var innerException = ex;
                while (innerException.InnerException != null)
                    innerException = innerException.InnerException;

                throw innerException;
            }
        }

        ObjectResult<T> IDataManager.Execute<T>(string sql, params object[] args)
        {
            var ret = default(ObjectResult<T>);
            ret = (this as IEntityGateway).ObjectContext.ExecuteStoreQuery<T>(sql, args);
            return ret;
        }

#if DEBUG
        public override string ToString()
        {
            return $"_map: {_map.ToString()}";
        }
#endif
    }
}
