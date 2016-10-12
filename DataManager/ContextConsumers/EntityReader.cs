using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using GenericDataManager.Interfaces;

namespace GenericDataManager.Consumers
{
    public class EntityReader<T> :
        ContextConsumerBase,
        IEntityReader<T> where T:class
    {
        internal EntityReader(IContextProvider arg):base(arg)
        {}


        IQueryable<T> IEntityReader<T>.All(System.Linq.Expressions.Expression<Func<T, bool>> expr)
        {
            return expr == null ? _provider.DataContext.Set<T>() : _provider.DataContext.Set<T>().Where(expr);
        }
        long IEntityReader<T>.Count(System.Linq.Expressions.Expression<Func<T, bool>> expr)
        {
            return expr == null ? _provider.DataContext.Set<T>().LongCount() : _provider.DataContext.Set<T>().LongCount(expr);
        }
        bool IEntityReader<T>.Exists(System.Linq.Expressions.Expression<Func<T, bool>> expr)
        {
            return _provider.DataContext.Set<T>().Count(expr)>0;
        }
        T IEntityReader<T>.One(System.Linq.Expressions.Expression<Func<T, bool>> expr)
        {
            var obj = expr == null ? _provider.DataContext.Set<T>().FirstOrDefault() : _provider.DataContext.Set<T>().Where(expr).FirstOrDefault();
            return obj;
        }
        IEnumerable<T> IEntityReader<T>.ReadOnly(System.Linq.Expressions.Expression<Func<T, bool>> expr)
        {
            var list = (expr == null ? 
                        _provider.DataContext.Set<T>() : 
                        _provider.DataContext.Set<T>().Where(expr)
                        ).AsNoTracking().ToList();

            return list;
        }
    }
}
