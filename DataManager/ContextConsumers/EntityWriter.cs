using System;
using System.Collections.Generic;
using System.Data;
#if EF5
using System.Data.EntityClient;
using System.Data.Objects;
#else
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.EntityClient;
#endif
using System.Linq;
using System.Linq.Expressions;
using GenericDataManager.Interfaces;

namespace GenericDataManager.Consumers
{
    public class EntityWriter<T> :
        ContextConsumerBase,
        IEntityWriter<T> where T : class
    {
        internal EntityWriter(IContextProvider arg):base(arg)
        {}

        void IEntityWriter<T>.Add(IEnumerable<T> arg)
        {
            
            foreach (var it in arg)
                _provider.DataContext.Set<T>().Add(it);

            Save(true);
         }
        void IEntityWriter<T>.Add(T arg)
        {
            _provider.DataContext.Set<T>().Add(arg);
            Save();
        }

        void IEntityWriter<T>.Delete(IEnumerable<T> list)
        {
            foreach(var it in list)
            {
                _provider.DataContext.Set<T>().Remove(it);
                _provider.ObjectContext.ObjectStateManager.ChangeObjectState(it, EntityState.Deleted);
            }
            _provider.DataContext.SaveChanges();
        }

        void IEntityWriter<T>.Delete(Expression<Func<T, bool>> expr)
        {
            var list = _provider.DataContext.Set<T>().Where(expr);
            foreach (var it in list)
                _provider.DataContext.Set<T>().Remove(it);
            Save(true);
        }
        void IEntityWriter<T>.Delete(T arg)
        {
            _provider.DataContext.Set<T>().Remove(arg);
            Save();
        }

        void IEntityWriter<T>.Update(IEnumerable<T> list)
        {
            foreach (var it in list)
                _provider.ObjectContext.ObjectStateManager.ChangeObjectState(it, EntityState.Modified);

            _provider.DataContext.SaveChanges();
        }

        void IEntityWriter<T>.Update(T arg)
        {
            _provider.ObjectContext.ObjectStateManager.ChangeObjectState(arg, EntityState.Modified);
            if (_provider.ObjectContext.ObjectStateManager.GetObjectStateEntry(arg).State == EntityState.Modified)
                Save();
        }
        void IEntityWriter<T>.Update(Expression<Func<T, bool>> expr, Action<T> action)
        {
            var list = _provider.DataContext.Set<T>().Where(expr);
            foreach (var it in list)
            {
                action(it);
                _provider.ObjectContext.ObjectStateManager.ChangeObjectState(it, EntityState.Modified);
            }
            Save(true);
        }
    }
}
