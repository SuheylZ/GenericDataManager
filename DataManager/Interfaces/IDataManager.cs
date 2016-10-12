using System;
#if EF5
using System.Data.EntityClient;
using System.Data.Objects;
#else
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.EntityClient;
#endif
namespace GenericDataManager.Interfaces
{
    /// <summary>
    /// Interface for teh data manager to acquire Entity writer and custom Sql executer
    /// </summary>
    public interface IDataManager : IDisposable
    {
        /// <summary>
        /// Retrives a custom repository based on the Entity specified as generic parameter
        /// </summary>
        /// <typeparam name="TEntity">The entity type for which the Repository pattern is required</typeparam>
        /// <returns>A specific repository class for the TEntity type that can be used to perform database operations.</returns>
        IEntityReaderWriter<TEntity> Get<TEntity>() where TEntity : class;

        /// <summary>
        /// Executes your custom sql with supplied parameters, if any. Do not use for Bulk operations
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">the sql that you want to run directly</param>
        /// <param name="args">any parameters to be supplied to the sql</param>
        /// <returns></returns>
        ObjectResult<T> Execute<T>(string sql, params object[] args);
    }

}
