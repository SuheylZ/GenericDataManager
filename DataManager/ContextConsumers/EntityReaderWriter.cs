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



        //void IEntityWriter<TEntity>.Add(IEnumerable<TEntity> arg)
        //{

        //    foreach (var it in arg)
        //        _provider.DataContext.Set<TEntity>().Add(it);

        //    Save(true);
        //}
        //void IEntityWriter<TEntity>.Add(TEntity arg)
        //{
        //    _provider.DataContext.Set<TEntity>().Add(arg);
        //    Save();
        //}
        //void IEntityWriter<TEntity>.Delete(Expression<Func<TEntity, bool>> expr)
        //{
        //    var list = _provider.DataContext.Set<TEntity>().Where(expr);
        //    foreach (var it in list)
        //        _provider.DataContext.Set<TEntity>().Remove(it);
        //    Save(true);
        //}
        //void IEntityWriter<TEntity>.Delete(TEntity arg)
        //{
        //    _provider.DataContext.Set<TEntity>().Remove(arg);
        //    Save();
        //}
        //void IEntityWriter<TEntity>.Update(TEntity arg)
        //{
        //    _provider.ObjectContext.ObjectStateManager.ChangeObjectState(arg, EntityState.Modified);

        //    if (_provider.ObjectContext.ObjectStateManager.GetObjectStateEntry(arg).State == EntityState.Modified)
        //        Save();
        //}
        //void IEntityWriter<TEntity>.Update(Expression<Func<TEntity, bool>> expr, Action<TEntity> action)
        //{
        //    var list = _provider.DataContext.Set<TEntity>().Where(expr);
        //    foreach (var it in list)
        //    {
        //        action(it);
        //        _provider.ObjectContext.ObjectStateManager.ChangeObjectState(it, EntityState.Modified);
        //    }
        //    Save(true);
        //}
        //void IEntityWriter<TEntity>.Update(IEnumerable<TEntity> list)
        //{
        //    foreach (var it in list)
        //        _provider.ObjectContext.ObjectStateManager.ChangeObjectState(it, EntityState.Modified);

        //    _provider.DataContext.SaveChanges();
        //}
        //void IEntityWriter<TEntity>.Delete(IEnumerable<TEntity> list)
        //{
        //    foreach (var it in list)
        //    {
        //        _provider.DataContext.Set<TEntity>().Remove(it);
        //        _provider.ObjectContext.ObjectStateManager.ChangeObjectState(it, EntityState.Deleted);
        //    }
        //    _provider.DataContext.SaveChanges();
        //}
    }
}
