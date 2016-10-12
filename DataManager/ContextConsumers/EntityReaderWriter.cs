using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using GenericDataManager.Interfaces;

namespace GenericDataManager.Consumers
{
    public class EntityReaderWriter<TEntity> :
        EntityWriter<TEntity>, 
        IEntityReaderWriter<TEntity>
        where TEntity : class
    {
        internal EntityReaderWriter(IContextProvider arg) : base(arg) { }

        IQueryable<TEntity> IEntityReader<TEntity>.All(System.Linq.Expressions.Expression<Func<TEntity, bool>> expr)
        {
            return expr == null ? _provider.DataContext.Set<TEntity>() : _provider.DataContext.Set<TEntity>().Where(expr);
        }
        long IEntityReader<TEntity>.Count(System.Linq.Expressions.Expression<Func<TEntity, bool>> expr)
        {
            return expr == null ? _provider.DataContext.Set<TEntity>().LongCount() : _provider.DataContext.Set<TEntity>().LongCount(expr);
        }
        bool IEntityReader<TEntity>.Exists(System.Linq.Expressions.Expression<Func<TEntity, bool>> expr)
        {
            return _provider.DataContext.Set<TEntity>().Count(expr) > 0;
        }
        TEntity IEntityReader<TEntity>.One(System.Linq.Expressions.Expression<Func<TEntity, bool>> expr)
        {
            var obj = expr == null ? _provider.DataContext.Set<TEntity>().FirstOrDefault() : _provider.DataContext.Set<TEntity>().Where(expr).FirstOrDefault();
            return obj;
        }
        IEnumerable<TEntity> IEntityReader<TEntity>.ReadOnly(System.Linq.Expressions.Expression<Func<TEntity, bool>> expr)
        {
            var list = (expr == null ?
                        _provider.DataContext.Set<TEntity>() :
                        _provider.DataContext.Set<TEntity>().Where(expr)
                        ).AsNoTracking().ToList();

            return list;
        }

    }
}
