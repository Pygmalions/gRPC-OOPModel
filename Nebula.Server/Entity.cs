using System.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Nebula.Core;
using Nebula.Protocols;
using Prism.Injecting;

namespace Nebula.Server;

public class Entity : IEntity, IContainer
{
    /// <summary>
    /// The id of this entity.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Injection container of this entity.
    /// </summary>
    public readonly InjectionContainer Container = new();

    public readonly Domain Domain;
    
    /// <summary>
    /// Bind to a certain entity in a domain.
    /// </summary>
    public Entity(Domain domain, uint id)
    {
        Id = id;

        Domain = domain;
        
        // Add the domain container to the search chain.
        Container.Chain.Add(domain.Container);
        
        // Add this entity to the injection container.
        Container.Add(typeof(IEntity), this);
    }

    /// <summary>
    /// Entity data.
    /// </summary>
    public IData Data { get; }

    public object? Get(Type category, string? id = null)
    {
        var instance = Container.Get(category, id);
        if (instance != null)
            return instance;
        
        if (category.IsAssignableTo(typeof(Component)))
            instance = GetComponent(category);

        if (instance != null)
            Container.Add(category, instance, id);

        return instance;
    }

    /// <summary>
    /// Get a component from this entity.
    /// </summary>
    /// <param name="componentType">Type of the component, must be a generated gRPC client type.</param>
    /// <returns>Component client.</returns>
    /// <exception cref="InvalidOperationException">
    /// Throw if the component type is not a valid gRPC service type.
    /// </exception>
    /// <exception cref="Exception">
    /// Throw if failed to instantiate or connect to the component server.
    /// </exception>
    public Component GetComponent(Type componentType)
    {
        // Verify the component type.
        if (!componentType.IsAssignableTo(typeof(ClientBase)) ||
            componentType.DeclaringType!.GetField("__ServiceName", 
                BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) is not string componentName)
            throw new InvalidOperationException($"Class {componentType} is not a gRPC generated client type.");

        // Check the component cache.
        if (Container.Get(componentType) is Component component)
            return component;
        
        // Acquire the address of the component server.
        if (Domain.GetService<EntityService.EntityServiceClient>()
                .GetComponent(new ComponentId{Name = componentName}) 
            is not {HasUri:true} server)
            throw new Exception($"Failed to find component server '{componentName}'.");
        // Intercept it with an entity selector.
        var invoker = GrpcChannel.ForAddress(server.Uri).Intercept(new EntitySelector(Id));
        // Create a component client.
        var instance = Activator.CreateInstance(componentType, invoker) as Component ?? 
                       throw new Exception(
                           $"Failed to instantiate or connect to component server '{componentName}' on entity #{Id}");
        return instance;
    }

    /// <summary>
    /// Get a component from this entity.
    /// </summary>
    /// <typeparam name="TComponent">Type of the component, must be a generated gRPC client type.</typeparam>
    /// <returns>Component client.</returns>
    /// <exception cref="InvalidOperationException">
    /// Throw if <typeparam name="TComponent"/> is not a valid gRPC service type.
    /// </exception>
    /// <exception cref="Exception">
    /// Throw if failed to instantiate or connect to the component server.
    /// </exception>
    public TComponent GetComponent<TComponent>() where TComponent : class
        => GetComponent(typeof(TComponent)) as TComponent ?? throw new Exception(
            $"Failed to instantiate or connect to component server '{typeof(TComponent)}' on entity #{Id}");
}