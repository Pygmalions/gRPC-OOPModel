using Grpc.Core;
using Grpc.Net.Client;
using Nebula.Core;
using Nebula.Protocols;
using Prism.Injecting;

namespace Nebula.Server;

using Empty = Google.Protobuf.WellKnownTypes.Empty;
public class Domain : IDomain, IContainer
{
    /// <summary>
    /// Create an entity in this domain.
    /// </summary>
    /// <returns>New created entity.</returns>
    public IEntity CreateEntity()
        => SearchEntity(GetService<DomainService.DomainServiceClient>().CreateEntity(
               new Empty()).Id) ??
           throw new Exception("Failed to create a new entity.");
    
    /// <summary>
    /// Delete an entity from this domain.
    /// </summary>
    /// <param name="id">Id of the entity to delete.</param>
    /// <returns>
    /// Whether an entity is successfully deleted or not.
    /// </returns>
    public void DeleteEntity(uint id)
        => GetService<DomainService.DomainServiceClient>().DeleteEntity(new EntityId {Id = id});

    /// <summary>
    /// Search an entity according to its id.
    /// </summary>
    /// <param name="id">Id of the entity.</param>
    /// <returns>Found entity, or null if not found.</returns>
    public IEntity? SearchEntity(uint id)
    {
        // Search from the cache.
        if (Container.Get(typeof(IEntity), id.ToString()) is IEntity entity)
            return entity;
        // Verify entity.
        if (!GetService<DomainService.DomainServiceClient>().VerifyEntity(new EntityId() { Id = id }).Valid)
            return null;
        // Instantiate an entity client.
        entity = new Entity(this, id);
        Container.Add(typeof(IEntity), entity, id.ToString());
        return entity;
    }

    /// <summary>
    /// Get a service from this domain.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <returns>Service client.</returns>
    /// <exception cref="Exception">
    /// Throw if failed to instantiate the service client or failed to connect to the service server.
    /// </exception>
    public TService GetService<TService>() where TService : class
        => Get(typeof(TService)) as TService ?? throw new Exception(
            $"Can not find required service {typeof(TService)}.");

    /// <summary>
    /// Injection container of this domain.
    /// </summary>
    public readonly InjectionContainer Container = new();

    /// <summary>
    /// Get an injection form this container.
    /// </summary>
    public object? Get(Type category, string? id = null)
    {
        var instance = Container.Get(category, id);
        if (instance != null)
            return instance;

        if (category.IsAssignableTo(typeof(ClientBase)))
            instance = Activator.CreateInstance(category, _nexus) ?? 
                       throw new Exception($"Failed to instantiate or connect to service '{category}'.");
        if (category.IsAssignableTo(typeof(IEntity)) && uint.TryParse(id, out var numberId))
        {
            instance = SearchEntity(numberId);
        }

        if (instance != null)
            Container.Add(category, instance, id);

        return instance;
    }

    /// <summary>
    /// Connection to the nexus server.
    /// </summary>
    private readonly GrpcChannel _nexus;

    public Domain(GrpcChannel nexus)
    {
        _nexus = nexus;
        Container.Add(typeof(IDomain), this);
    }
}