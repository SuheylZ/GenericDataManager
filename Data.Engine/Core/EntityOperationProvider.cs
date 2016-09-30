using System;
using System.Collections.Generic;

using System.Linq;
using System.Linq.Expressions;

using DataManager.Interfaces;
using System.Diagnostics.Contracts;

namespace DataManager.Core
{
    class EntityOperationProvider<TEntity> :
        Interfaces.IContextConsumer, Interfaces.IEntityReaderWriter<TEntity>
        where TEntity:class
    {
        private int _key;
        private IContextProvider _provider;


        void IContextConsumer.Consume(IContextProvider arg, int key)
        {
            _provider = arg;
            _key = key;
        }
        void IDisposable.Dispose()
        {
            _provider.Release(_key);
        }



        IQueryable<TEntity> IEntityReader<TEntity>.All(Expression<Func<TEntity, bool>> expr)
        {
            Contract.Requires(_provider.Set<TEntity>() != null);

            return expr == null ?
                _provider.Set<TEntity>() : _provider.Set<TEntity>().Where(expr); 
        }
        long IEntityReader<TEntity>.Count(Expression<Func<TEntity, bool>> expr)
        {
            return expr == null ?
               _provider.Set<TEntity>().AsNoTracking().LongCount() : _provider.Set<TEntity>().AsNoTracking().LongCount(expr);

        }
        IQueryable<TEntity> IEntityReader<TEntity>.ReadOnly(Expression<Func<TEntity, bool>> expr)
        {
            return expr == null ?
            _provider.Set<TEntity>().AsNoTracking() : _provider.Set<TEntity>().AsNoTracking().Where(expr);

        }
        bool IEntityReader<TEntity>.Exists(Expression<Func<TEntity, bool>> expr)
        {
            return _provider.Set<TEntity>().AsNoTracking().Count(expr) > 0; 
        }
        TEntity IEntityReader<TEntity>.One(Expression<Func<TEntity, bool>> expr)
        {
            return expr == null ? _provider.Set<TEntity>().FirstOrDefault() : _provider.Set<TEntity>().FirstOrDefault(expr);
        }



        void IEntityWriter<TEntity>.Add(TEntity arg)
        {
            _provider.Set<TEntity>().Add(arg);
            _provider.Save();
        }
        void IEntityWriter<TEntity>.Add(IEnumerable<TEntity> arg)
        {
            _provider.Set<TEntity>().AddRange(arg);
            _provider.Save();
        }
        void IEntityWriter<TEntity>.Update(TEntity arg)
        {
            _provider.Save();
        }
        void IEntityWriter<TEntity>.Update(Expression<Func<TEntity, bool>> expr, Action<TEntity> statement)
        {
            var items = _provider.Set<TEntity>().Where(expr);
            foreach (var it in items)
                statement(it);
            _provider.Save();
        }
        void IEntityWriter<TEntity>.Delete(Expression<Func<TEntity, bool>> expr)
        {
            var list = _provider.Set<TEntity>().Where(expr);
            _provider.Set<TEntity>().RemoveRange(list);
            _provider.Save();
        }
        void IEntityWriter<TEntity>.Delete(TEntity arg)
        {
            _provider.Set<TEntity>().Remove(arg);
            _provider.Save();
        }
    }
}
