namespace Logic.Transport.Abstractions;

public interface IDeserializer<TEntity>
    where TEntity : class
{
    public TEntity Deserialize(string source);
}
