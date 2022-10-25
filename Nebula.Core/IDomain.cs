namespace Nebula.Core;

public interface IDomain
{
    /// <summary>
    /// Create an entity.
    /// </summary>
    /// <returns>Created entity.</returns>
    IEntity CreateEntity();

    /// <summary>
    /// Delete an entity.
    /// </summary>
    /// <param name="id">Id of the entity.</param>
    void DeleteEntity(uint id);

    /// <summary>
    /// Search an entity.
    /// </summary>
    /// <param name="id">Id of the entity.</param>
    /// <returns>Entity with the specified ID or null if not found.</returns>
    IEntity? SearchEntity(uint id);

    /// <summary>
    /// Get a service from this domain.
    /// </summary>
    /// <typeparam name="TService">Type of the service.</typeparam>
    /// <returns>Service instance.</returns>
    TService GetService<TService>() where TService : class;
}