namespace Logic.Transport.Abstractions;

public interface ISerializer<TEntity>
    where TEntity : class
{
    public string Serialize(TEntity entity);
}
