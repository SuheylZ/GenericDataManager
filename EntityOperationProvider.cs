using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DataManager.Common;
using DataManager.Interfaces;

namespace DataManager.Core
{
    class EntityOperationProvider<TEntity> : Interfaces.IEntityGatewayClient, Interfaces.IEntityOperations<TEntity>
        where TEntity:class
    {
        private string _key;
        private IEntityGateway _manager;
        private static QuickLock _access = new QuickLock();


        void IEntityWriter<TEntity>.Add(TEntity arg)
        {
            _access.Lock();
           _manager.Set<TEntity>().Add(arg);
            _manager.Save();
           _access.Unlock();
        }
        void IEntityWriter<TEntity>.Add(IEnumerable<TEntity> arg)
        {
            _access.Lock();
            _manager.Set<TEntity>().AddRange(arg);
            _manager.Save();
            _access.Unlock();
        }

        IQueryable<TEntity> IEntityReader<TEntity>.All(Expression<Func<TEntity, bool>> expr)
        {
            return expr==null?
                _manager.Set<TEntity>(): _manager.Set<TEntity>().Where(expr);
        }

        long IEntityReader<TEntity>.Count(Expression<Func<TEntity, bool>> expr)
        {
            return expr == null ?
               _manager.Set<TEntity>().AsNoTracking().LongCount() : _manager.Set<TEntity>().AsNoTracking().LongCount(expr);
        }

        void IEntityWriter<TEntity>.Delete(Expression<Func<TEntity, bool>> expr)
        {
            _access.Lock();

            var list = _manager.Set<TEntity>().Where(expr);
            _manager.Set<TEntity>().RemoveRange(list);
            _manager.Save();

            _access.Unlock();

        }

        void IEntityWriter<TEntity>.Delete(TEntity arg)
        {
            _access.Lock();

            _manager.Set<TEntity>().Remove(arg);
            _manager.Save();

            _access.Unlock();
        }

        void IDisposable.Dispose()
        {
            _manager.Release(_key);
        }

        bool IEntityReader<TEntity>.Exists(Expression<Func<TEntity, bool>> expr)
        {
            return _manager.Set<TEntity>().AsNoTracking().Count(expr)>0;
        }

        TEntity IEntityReader<TEntity>.One(Expression<Func<TEntity, bool>> expr)
        {
            return expr == null ?
           _manager.Set<TEntity>().FirstOrDefault(): _manager.Set<TEntity>().FirstOrDefault(expr);
        }

        void IEntityGatewayClient.Register(IEntityGateway arg, string key, TimeSpan duration)
        {
            Debug.Assert(arg!= null);
            _manager = arg;
            _key = key;
            _access = new QuickLock(duration);
        }

        void IEntityWriter<TEntity>.Update(TEntity arg)
        {
            _access.Lock();

            _manager.Save();

            _access.Unlock();
        }

        void IEntityWriter<TEntity>.Update(Expression<Func<TEntity, bool>> expr, Action<TEntity> statement)
        {
            
            var items = _manager.Set<TEntity>().Where(expr);

            _access.Lock();

            foreach (var it in items)
                statement(it);
            _manager.Save();

            _access.Unlock();
        }

        IQueryable<TEntity> IEntityReader<TEntity>.ReadOnly(Expression<Func<TEntity, bool>> expr)
        {
            return expr == null ?
             _manager.Set<TEntity>().AsNoTracking() : _manager.Set<TEntity>().AsNoTracking().Where(expr);
        }
    }
}
