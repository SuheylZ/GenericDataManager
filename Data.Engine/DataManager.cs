using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using DataManager.Interfaces;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;

namespace DataManager.Core
{
    /// <summary>
    /// DataManager keeps track of all EntityOperations and provides a single instantiation of EntityOperation of each particular entity.
    /// </summary>
    public class DataManager : Interfaces.IEntityGateway, IDataManager
    {
        class CountableItem{
            object _item = null;

            internal int Count {get; private set; }
            internal object Object {
                get {
                    Count++;
                    return _item;
                }
            }
            internal CountableItem(object arg) {
                Debug.Assert(arg!= null);
                _item = arg;
                Count =1;
            }

            internal void Free() {
                Count--;
            }
        }

        DbContext _ctx = null;
        private static object _mapLock = new object();
        Dictionary<string, CountableItem> _map = new Dictionary<string, CountableItem>();
        readonly TimeSpan _maxWait = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Establishes the connection and initializes internal structures
        /// </summary>
        /// <param name="connectionStr">basic connection string</param>
        /// <param name="model">the name of the model without the extensions</param>
        /// <param name="provider">provider name, default is System.Data.SqlClient</param>
        public DataManager(string connectionStr, string model, string provider = "System.Data.SqlClient") {
            var builder = new EntityConnectionStringBuilder();
            builder.ProviderConnectionString = connectionStr;
            builder.Provider = provider;
            builder.Metadata = string.Format(@"res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", model); 
            _ctx = new DbContext(builder.ToString());
            _ctx.Database.Connection.Open();
        }


        /// <summary>
        /// Establishes the connection and initializes internal structures
        /// </summary>
        /// <param name="connectionStr">basic connection string</param>
        /// <param name="model">the name of the model without the extensions</param>
        /// <param name="provider">provider name, default is System.Data.SqlClient</param>
        /// <param name="maxWait">The maximum duration for after which the operation must fail if lock could not be acquired.</param>
        public DataManager(string connectionStr, string model, TimeSpan maxWait, string provider = "System.Data.SqlClient")
        {
            _maxWait = maxWait;

            var builder = new EntityConnectionStringBuilder();
            builder.ProviderConnectionString = connectionStr;
            builder.Provider = provider;
            builder.Metadata = string.Format(@"res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", model);
            _ctx = new DbContext(builder.ToString());
            _ctx.Database.Connection.Open();
        }

        void IEntityGateway.Release(string id)
        {
            lock(_mapLock) {
                _map[id].Free();
            }
        }
        /// <summary>
        /// Retrieves the EntityOperation for a particular entity type. Only a single instance is instantiated
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IEntityReaderWriter<TEntity> IDataManager.Get<TEntity>() {
            IEntityReaderWriter<TEntity> ret = null;

            var key = typeof(TEntity).FullName;
            lock(_mapLock) {
                if(!_map.ContainsKey(key)) {
                    var item = new EntityOperationProvider<TEntity>();
                    (item as Interfaces.IEntityGatewayClient).Register(this, key, _maxWait);
                    _map.Add(key, new CountableItem(item));
                }
                ret = _map[key].Object as IEntityReaderWriter<TEntity>;
            }
            return ret;
        }

        void IDisposable.Dispose()
        {
            lock(_mapLock) {
               _map.Clear();
            }
            _ctx.Dispose();
            _ctx = null;
        }

        DbSet<TEntity> IEntityGateway.Set<TEntity>()
        {
            return _ctx.Set<TEntity>();
        }

        ObjectContext IEntityGateway.ObjectContext
        {
            get
            {
                ObjectContext ret = null;
                var adapter = _ctx as IObjectContextAdapter;
                if(adapter != null)
                    ret = adapter.ObjectContext;
                return ret;
            }
        }

        DbContext IEntityGateway.Context
        {
            get
            {
               return _ctx;
            }
        }

        void IEntityGateway.Save()
        {
                _ctx.SaveChanges();
        }

        ObjectResult<T> IDataManager.Execute<T>(string sql, params object[] args)
        {
            var ret = default(ObjectResult<T>);
            ret = (this as IEntityGateway).ObjectContext.ExecuteStoreQuery<T>(sql, args);
            return ret;
        }
    }
}
