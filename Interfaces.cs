using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace DataManager.Interfaces
{
    public interface IEntityGateway {

        ObjectContext ObjectContext {get; }
        DbContext Context { get; }

        DbSet<TEntity> Set<TEntity>() where TEntity: class;
        void Save();

        void Release(string id);
    }

    public interface IEntityGatewayClient {
        void Register(IEntityGateway arg, string key, TimeSpan duration);
    }

    public interface IEntityReader<TEntity>: IDisposable where TEntity:class
    {
        TEntity One(Expression<Func<TEntity, bool>> expr = null);
        IQueryable<TEntity> All(Expression<Func<TEntity, bool>> expr = null);
        long Count(Expression<Func<TEntity, bool>> expr = null);
        bool Exists(Expression<Func<TEntity, bool>> expr = null);
        IQueryable<TEntity> ReadOnly(Expression<Func<TEntity, bool>> expr = null);
    }
    public interface IEntityWriter<TEntity> : IDisposable where TEntity : class
    {
        void Add(TEntity arg);
        void Add(IEnumerable<TEntity> arg);
        void Update(TEntity arg);
        void Update(Expression<Func<TEntity, bool>> expr, Action<TEntity> statement);
        void Delete(TEntity arg);
        void Delete(Expression<Func<TEntity, bool>> expr);
    }



    public interface IEntityOperations<TEntity> : 
        IEntityReader<TEntity>, 
        IEntityWriter<TEntity>
        where TEntity : class
    {

    }

}
