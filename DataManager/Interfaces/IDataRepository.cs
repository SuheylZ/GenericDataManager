namespace GenericDataManager.Interfaces
{
    public interface IDataRepository
    {
        IEntityReader<TEntity> GetReader<TEntity>() where TEntity : class;
        IEntityWriter<TEntity> GetWriter<TEntity>() where TEntity : class;
        IEntityReaderWriter<TEntity> Get<TEntity>() where TEntity : class;
        ISqlDirect Sql { get; }
    }
}
