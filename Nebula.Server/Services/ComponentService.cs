using Grpc.Core;
using Nebula.Core;
using Nebula.Protocols;
using Prism.Framework;
using Prism.Injecting;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace Nebula.Server.Services;

public class ComponentService : Protocols.ComponentService.ComponentServiceBase
{
    private readonly Dictionary<uint, Component> _components = new();

    public override Task<Empty> AttachComponent(Empty request, ServerCallContext context)
    {
        return Task.Run(() =>
        {
            var entity = context.GetEntity();
            if (_components.TryGetValue(entity.Id, out _))
                return new Empty();
            if (Activator.CreateInstance(Server.This.ComponentType) as Component is not { } component)
                throw new Exception($"Failed to instantiate component {Server.This.ComponentType}.");
            component.As<IInjectable>().Inject(entity);
            _components[entity.Id] = component;
            component.OnAttach();
            return new Empty();
        });
    }

    public override Task<Empty> DetachComponent(Empty request, ServerCallContext context)
    {
        var entityId = context.GetEntityId();
        if (_components.Remove(entityId, out var component))
            component.OnDetach();
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> ConfigureComponent(Empty request, ServerCallContext context)
    {
        return Task.Run(() =>
        {
            if (_components.TryGetValue(context.GetEntityId(), out var component))
                component.OnConfigure();
            return new Empty();
        });
    }

    public override Task<PropertyList> ListProperties(Empty request, ServerCallContext context)
    {
        if (_components.TryGetValue(context.GetEntityId(), out var component))
            return Task.FromResult(component.As<IMonitorHost>().ListProperties());
        context.Status = new Status(StatusCode.NotFound, $"Can not find entity #{context.GetEntityId()}.");
        return Task.FromResult(new PropertyList());
    }

    public override Task<PropertyContent> GetPropertyData(PropertyReadingRequest request, ServerCallContext context)
    {
        if (_components.TryGetValue(context.GetEntityId(), out var component))
            return Task.FromResult(component.As<IMonitorHost>().GetPropertyData(request));
        context.Status = new Status(StatusCode.NotFound, $"Can not find entity #{context.GetEntityId()}.");
        return Task.FromResult(new PropertyContent());
    }

    public override Task<Empty> SetPropertyData(PropertyContent request, ServerCallContext context)
    {
        if (_components.TryGetValue(context.GetEntityId(), out var component))
        {
            component.As<IMonitorHost>().SetPropertyData(request);
            return Task.FromResult(new Empty());
        }
            
        context.Status = new Status(StatusCode.NotFound, $"Can not find entity #{context.GetEntityId()}.");
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> StartMonitor(PropertyMonitorRequest request, ServerCallContext context)
    {
        if (_components.TryGetValue(context.GetEntityId(), out var component))
        {
            component.As<IMonitorHost>().StartMonitor(request);
            return Task.FromResult(new Empty());
        }
            
        context.Status = new Status(StatusCode.NotFound, $"Can not find entity #{context.GetEntityId()}.");
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> StopMonitor(PropertyId request, ServerCallContext context)
    {
        if (_components.TryGetValue(context.GetEntityId(), out var component))
        {
            component.As<IMonitorHost>().StopMonitor(request);
            return Task.FromResult(new Empty());
        }
            
        context.Status = new Status(StatusCode.NotFound, $"Can not find entity #{context.GetEntityId()}.");
        return Task.FromResult(new Empty());
    }
}