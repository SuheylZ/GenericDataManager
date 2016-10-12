using System;
using System.Collections.Generic;
using System.Data.Entity;

#if EF5
using System.Data.EntityClient;
using System.Data.Objects;
#else
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.EntityClient;
#endif

using System.Linq;
using System.Linq.Expressions;

namespace GenericDataManager.Interfaces
{
    public interface IDestroyable
    {
        void Destroy();
    }


    public interface IEntityGateway {

        ObjectContext ObjectContext {get; }
        DbContext Context { get; }

        DbSet<TEntity> Set<TEntity>() where TEntity: class;
        void Save();

        void Release(int id);
    }

    /// <summary>
    /// An interface that must be implemented by the class that provides the IEntityReader or IEntityWriter interfaces. 
    /// This interface helps the class to get get the db context, concurrency and reference counting provided by the manager
    /// </summary>
    public interface IEntityGatewayClient {
        /// <summary>
        /// A client class class to get itself registered with the manager
        /// </summary>
        /// <param name="arg">An implementation (the class itself usually) that the manager uses to handle its instantiation and concurrency issues</param>
        /// <param name="key">A constant key to be provided by the class. A GUID for instance</param>
        /// <param name="duration">duration for which a class can hold a lock if necessary</param>
        void Register(IEntityGateway arg, int key);
    }


    public interface IContextProvider: IDisposable
    {
        DbContext DataContext { get; }
        ObjectContext ObjectContext { get; }
        void Release(IContextConsumer arg, string key);
    }
    public interface IContextConsumer
    {
        void Init(IContextProvider arg, string key);
        void Cleanup();
    }


    public interface IEntityReader<TEntity>: IDisposable where TEntity:class
    {
        /// <summary>
        /// Retrieves a single record or 1st record if there are many, based on the expression provided
        /// </summary>
        /// <param name="expr">Linq expression to be used to get a record</param>
        /// <returns>record that satisfies the condition or the first record if there are many</returns>
        TEntity One(Expression<Func<TEntity, bool>> expr = null);
        /// <summary>
        /// Returns all records that satisfy a condition
        /// </summary>
        /// <param name="expr">linq expression to retrieve the records</param>
        /// <returns>a querable list of records</returns>
        IQueryable<TEntity> All(Expression<Func<TEntity, bool>> expr = null);
        /// <summary>
        /// Counts the number of records satifying a particular condition
        /// </summary>
        /// <param name="expr">Linq expression to use to filter records</param>
        /// <returns>returns the number of records that satisfied the condition</returns>
        long Count(Expression<Func<TEntity, bool>> expr = null);
        /// <summary>
        /// Checks if there is any record that satisfies the condtion
        /// </summary>
        /// <param name="expr">linq expression to be used to verify</param>
        /// <returns></returns>
        bool Exists(Expression<Func<TEntity, bool>> expr = null);
        IEnumerable<TEntity> ReadOnly(Expression<Func<TEntity, bool>> expr = null);
    }
    public interface IEntityWriter<TEntity> : IDisposable where TEntity : class
    {
        /// <summary>
        /// Adds an entity to the database
        /// </summary>
        /// <param name="arg">Entity record to be added</param>
        void Add(TEntity arg);
        /// <summary>
        /// Adds a list of records to the database
        /// </summary>
        /// <param name="arg">a list of records to be saved.</param>
        void Add(IEnumerable<TEntity> arg);
        /// <summary>
        /// Saves the changes made to an entity
        /// </summary>
        /// <param name="arg">record whose changes are to be saved</param>
        void Update(TEntity arg);
        void Update(IEnumerable<TEntity> list);
        /// <summary>
        /// Updates a list of records that satisfy a particular expression[example: Update tbl Set tbl.X =?? Where  tbl.Y = ? and tbl.Z = ?]
        /// </summary>
        /// <param name="expr">Linq expression to be used</param>
        /// <param name="statement">Action which update the records</param>
        void Update(Expression<Func<TEntity, bool>> expr, Action<TEntity> statement);
        /// <summary>
        /// Removes a record from the daatabase
        /// </summary>
        /// <param name="arg">record to be removed</param>
        void Delete(TEntity arg);
        void Delete(IEnumerable<TEntity> list);
        /// <summary>
        /// Removes records that satisfy a particular expression [eg: Delete tbl Where tbl.X=? and tbl.Y < ?]
        /// </summary>
        /// <param name="expr">linq predicate that the records must satisfy for deletion</param>
        void Delete(Expression<Func<TEntity, bool>> expr);
    }
    public interface IEntityReaderWriter<TEntity> :
        IEntityReader<TEntity>,
        IEntityWriter<TEntity>
        where TEntity : class
    {
    }
}
