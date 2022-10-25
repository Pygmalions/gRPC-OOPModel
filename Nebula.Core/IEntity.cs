using MongoDB.Bson;

namespace Nebula.Core;

public interface IEntity
{
    /// <summary>
    /// The id of this entity.
    /// </summary>
    public uint Id { get; }
    
    /// <summary>
    /// Entity data.
    /// </summary>
    public IData Data { get; }

    /// <summary>
    /// Get a component from this entity.
    /// </summary>
    /// <typeparam name="TComponent">Type of the component.</typeparam>
    /// <returns>Component client.</returns>
    TComponent GetComponent<TComponent>() where TComponent : class;
}