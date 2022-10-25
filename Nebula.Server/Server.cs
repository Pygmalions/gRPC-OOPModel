using System.Reflection;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MongoDB.Driver;
using Nebula.Core;
using Nebula.Protocols;
using Prism.Injecting;

namespace Nebula.Server;

public class Server 
{
    public static Server This { get; internal set; } = null!;

    /// <summary>
    /// Port for this server to use.
    /// </summary>
    public readonly int Port;

    /// <summary>
    /// Type of the gRPC service.
    /// </summary>
    public readonly Type ServiceType;

    public readonly Type ComponentType;

    /// <summary>
    /// Service name.
    /// </summary>
    public readonly string Name;
    
    /// <summary>
    /// Name of the isolated environment for this server to use.
    /// </summary>
    public readonly string? Environment;

    /// <summary>
    /// Database which this server is using.
    /// </summary>
    public IMongoDatabase Database;
    
    /// <summary>
    /// Domain of this server.
    /// </summary>
    public readonly Domain Domain;

    public Server(Uri nexus, int port, Type component, string? environment)
    {
        Domain = new Domain(GrpcChannel.ForAddress(nexus));
        
        Port = port;
        Environment = environment ?? "Nebula";

        var generator = new Prism.Framework.Generator();
        generator.RegisterPlugin(new InjectionPlugin());
        generator.RegisterPlugin(new MonitorPlugin());

        ComponentType = generator.GetProxy(component);
        
        if (ComponentType.GetCustomAttribute<ComponentAttribute>() is not { } attribute)
            throw new Exception($"Component type {ComponentType} does not have a valid {nameof(ComponentAttribute)}.");
        
        ServiceType = attribute.Base;
        if (!ServiceType.IsAssignableTo(typeof(ClientBase)) ||
            ServiceType.DeclaringType!.GetField("__ServiceName", 
                BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) is not string serviceName)
            throw new InvalidOperationException($"Class {ServiceType} is not a gRPC generated client type.");
        Name = serviceName;
        
        if (Domain.GetService<DomainService.DomainServiceClient>()
                .SearchServer(new ServerName() { Name = "Nebula.Database" })
            is not { HasUri: true } databaseAddress)
            throw new Exception("Can not find an address of the database.");
        Database = new MongoClient(databaseAddress.Uri).GetDatabase(Environment);
    }

    /// <summary>
    /// Cancellation token source to stop the server.
    /// </summary>
    private CancellationTokenSource? _lifeSource;
    
    /// <summary>
    /// Start this server.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task Start()
    {
        if (_lifeSource != null)
            throw new InvalidOperationException("Server is already running.");

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(Port, listening =>
            {
                listening.Protocols = HttpProtocols.Http2;
            });
        });
        
        // Add services to the container.
        builder.Services.AddGrpc();

        var application = builder.Build();
        
        typeof(GrpcEndpointRouteBuilderExtensions)
            .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService))!
            .MakeGenericMethod(ServiceType)
            .Invoke(null, new object?[] { application });
        application.MapGrpcService<Services.ComponentService>();
        
        _lifeSource = new CancellationTokenSource();
        await application.RunAsync(_lifeSource.Token);
    }

    /// <summary>
    /// Stop this server.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Stop()
    {
        if (_lifeSource == null)
            throw new InvalidOperationException("Server is not running.");
        _lifeSource.Cancel();
        _lifeSource = null;
    }
}