using MongoDB.Bson;
using MQTTnet.Client;
using Prism.Injecting;

namespace Nebula.Core;

public abstract class Component
{
    /// <summary>
    /// The domain of the entity.
    /// </summary>
    [Inject(necessary: true)]
    public IDomain Domain { get; protected set; } = null!;

    /// <summary>
    /// The entity that this component is attached to.
    /// </summary>
    [Inject(necessary: true)]
    public IEntity Entity { get; protected set; } = null!;

    /// <summary>
    /// Component data.
    /// </summary>
    [Inject(necessary: true)]
    public IData Data { get; protected set; } = null!;

    /// <summary>
    /// Client for the MQTT message service.
    /// </summary>
    [Inject(necessary: true)]
    public MqttClient Message { get; protected set; } = null!;

    /// <summary>
    /// Logger for this component to use.
    /// </summary>
    [Inject(necessary: true)]
    public ILogger Log { get; protected set; } = null!;

    /// <summary>
    /// Triggered when this component is attached to an entity.
    /// </summary>
    public virtual void OnAttach()
    {}

    /// <summary>
    /// Triggered when this component is detached from an entity.
    /// </summary>
    public virtual void OnDetach()
    {}

    public virtual void OnConfigure()
    {}
}