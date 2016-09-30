using System;

using DataManager.Interfaces;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Threading;
using DataManager.Common;
using System.Diagnostics.Contracts;

namespace DataManager.Core
{

    /// <summary>
    /// DataManager keeps track of all EntityOperations and provides a single instantiation of EntityOperation of each particular entity.
    /// </summary>
    public class DataManager : Interfaces.IContextProvider, IDataManager
    {
        ConnectionParameters KConnectionParameters;
        static ThreadMap4Items<int, object> _map;

        /// <summary>
        /// Establishes the connection and initializes internal structures
        /// </summary>
        /// <param name="arg">structure that holds the connection information</param>
        public DataManager(ConnectionParameters arg)
        {
            Contract.Requires(!string.IsNullOrEmpty(arg.connection));
            Contract.Requires(!string.IsNullOrEmpty(arg.modelResource));

            KConnectionParameters = arg;
            _map = new ThreadMap4Items<int, object>(16, (obj)=> (obj as IDisposable).Dispose());
            _map.Add(Thread.CurrentThread.ManagedThreadId, CreateDbContext());
        }

        void IContextProvider.Release(int id)
        {
            if (!_map.Has(id))
                throw new InvalidOperationException("Removal request for an Operation provider whose record is missing");

            _map.Free(id);
            if (_map.ReferenceCount(id) < 1)
                _map.Delete(id);
        }

        /// <summary>
        /// Retrieves the EntityOperation for a particular entity type. Only a single instance is instantiated
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IEntityReaderWriter<TEntity> IDataManager.Get<TEntity>()
        {
            Contract.Ensures(Contract.Result<IEntityReaderWriter<TEntity>>() != null);

            var threadId = Thread.CurrentThread.ManagedThreadId;
            var provider = (this as IContextProvider);
            
            IEntityReaderWriter<TEntity> ret = null;
            if (!_map.Has(threadId))
                _map.Add(threadId, CreateDbContext());

            ret = new EntityOperationProvider<TEntity>();
            (ret as Interfaces.IContextConsumer).Consume(this, threadId);

            return ret;
        }

        void IDisposable.Dispose()
        {
            _map.Clear();
        }

        DbSet<TEntity> IContextProvider.Set<TEntity>()
        {
            Contract.Requires((this as IContextProvider).Context != null);
            return (this as IContextProvider).Context.Set<TEntity>();
        }

        ObjectContext IContextProvider.ObjectContext
        {
            get
            {
                Contract.Requires((this as IContextProvider).Context != null);

                ObjectContext ret = null;
                var adapter = (this as IContextProvider).Context as IObjectContextAdapter;
                if(adapter != null)
                    ret = adapter.ObjectContext;
                return ret;
            }
        }

        DbContext IContextProvider.Context
        {
            get
            {
                var value = _map[Thread.CurrentThread.ManagedThreadId] as DbContext;
                return value;
            }
        }

        private DbContext CreateDbContext()
        {
            Contract.Ensures(Contract.Result<DbContext>()!=null);

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

        void IContextProvider.Save()
        {
            Contract.Requires((this as IContextProvider).Context != null);

            try
            {
                (this as IContextProvider).Context.SaveChanges();
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;

                throw ex; 
            }
        }

        ObjectResult<T> IDataManager.Execute<T>(string sql, params object[] args)
        {
            var ret = (this as IContextProvider).ObjectContext.ExecuteStoreQuery<T>(sql, args);
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
