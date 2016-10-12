using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;

using System.Data.Entity.Infrastructure;

#if EF5
using System.Data.EntityClient;
using System.Data.Objects;
#else
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.EntityClient;
#endif

using System.Threading;
using GenericDataManager.Common;
using GenericDataManager.Consumers;
using GenericDataManager.Interfaces;

namespace GenericDataManager.Providers
{
    internal class ContextProviderBase : 
        IDataRepository,
        IContextProvider, 
        IDisposable
    {
        protected readonly DbContext _ctx;
        protected readonly ObjectContext _objctx;
        protected readonly IDictionary<string, IContextConsumer> _map;
        protected readonly int _threadId;


        readonly DateTime _createdAt;

        internal ContextProviderBase(ConnectionParameters arg)
        {
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _createdAt = DateTime.Now;

            _map = new SortedDictionary<string, IContextConsumer>();

            var builder = new EntityConnectionStringBuilder
            {
                ProviderConnectionString = arg.connection,
                Metadata = arg.modelResource,
                Provider = arg.provider
            };

            if (!builder.ProviderConnectionString.Contains(Konstants.MultipleActiveRecordSets))
            {
                if (builder.ProviderConnectionString.EndsWith(";"))
                    builder.ProviderConnectionString += $"{Konstants.MultipleActiveRecordSets};";
                else
                    builder.ProviderConnectionString += $";{Konstants.MultipleActiveRecordSets};";
            }
                
            _ctx = new DbContext(builder.ToString());
            var adapter = _ctx as IObjectContextAdapter;
            if (adapter != null)
                _objctx = adapter.ObjectContext;
            else
                throw new InvalidOperationException($"Could not create object context from the DbContext on {Thread.CurrentThread.ManagedThreadId}");

            EnsureWorkingConnection();
        }

        internal int ThreadId => _threadId;
        internal int ConsumerCount => _map.Count;

        protected void EnsureWorkingConnection()
        {
            if (_objctx.Connection.State != ConnectionState.Open)
                _objctx.Connection.Open();
        }


        private string Key<T1, T2>() => $"{typeof(T1).Name}:{typeof(T2).Name}";
        public virtual DbContext DataContext => _ctx;
        public virtual ObjectContext ObjectContext => _objctx;



        public virtual void Release(IContextConsumer arg, string key)
        {
            if (!_map.ContainsKey(key))
                throw new ObjectDisposedException($"ContextConsumer could not be found. Error in thread {Thread.CurrentThread.ManagedThreadId}");

            _map.Remove(key);
        }

        public void Dispose()
        {
            var keys = _map.Keys;
            foreach (var key in keys)
            {
                var consumer = _map[key];
                consumer.Dispose();
            }

            _map.Clear();

            _objctx.Connection.Close();

            _objctx.Dispose();
            _ctx.Dispose();
        }



        ISqlDirect IDataRepository.Sql
        {
            get
            {
                var key = Key<ISqlDirect, Object>();
                if (!_map.ContainsKey(key))
                {
                    var consumer = new SqlDirect(this);
                    (consumer as IContextConsumer).Init(this, key);
                    _map.Add(key, consumer);
                }
                return _map[key] as ISqlDirect;
            }
        }
        IEntityReader<TYPE> IDataRepository.GetReader<TYPE>()
        {
            var key = Key<IEntityReader<TYPE>, TYPE>();
            if (!_map.ContainsKey(key))
            {
                var consumer = new EntityReader<TYPE>(this);
                (consumer as IContextConsumer).Init(this, key);
                _map.Add(key, consumer);
            }
            return _map[key] as IEntityReader<TYPE>;
        }
        IEntityWriter<TEntity> IDataRepository.GetWriter<TEntity>() 
        {
            var key = Key<IEntityWriter<TEntity>, TEntity>();
            if (!_map.ContainsKey(key))
            {
                var consumer = new EntityWriter<TEntity>(this);
                (consumer as IContextConsumer).Init(this, key);
                _map.Add(key, consumer);
            }
            return _map[key] as IEntityWriter<TEntity>;
        }
        IEntityReaderWriter<TEntity> IDataRepository.Get<TEntity>()
        {
            var key = Key<IEntityWriter<TEntity>, TEntity>();
            if (!_map.ContainsKey(key))
            {
                var consumer = new EntityReaderWriter<TEntity>(this);
                (consumer as IContextConsumer).Init(this, key);
                _map.Add(key, consumer);
            }
            return _map[key] as IEntityReaderWriter<TEntity>;
        }
    }
}
