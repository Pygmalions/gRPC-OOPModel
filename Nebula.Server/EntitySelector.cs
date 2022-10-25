using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Nebula.Core;
using Nebula.Protocols;

namespace Nebula.Server;

/// <summary>
/// This interceptor is used to assign Entity Id to the invocation header.
/// </summary>
public class EntitySelector : Interceptor
{
    private readonly byte[] _idData;
    
    public EntitySelector(uint id)
    {
        // _idData = new Protocols.EntityId { Id = id }.ToByteArray();
    }

    private void BindEntity<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse>  context) 
        where TRequest : class
        where TResponse : class
    {
        if (context.Options.Headers == null)
        {
            context = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method, context.Host, context.Options.WithHeaders(new Metadata()
                {
                    {"eid-bin", _idData}
                }));
        } else context.Options.Headers.Add("eid-bin", _idData);
    }
    
    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        BindEntity(ref context);
        return base.BlockingUnaryCall(request, context, continuation);
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        BindEntity(ref context);
        return base.AsyncUnaryCall(request, context, continuation);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        BindEntity(ref context);
        return base.AsyncServerStreamingCall(request, context, continuation);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        BindEntity(ref context);
        return base.AsyncClientStreamingCall(context, continuation);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        BindEntity(ref context);
        return base.AsyncDuplexStreamingCall(context, continuation);
    }
}

public static class EntitySelectorHelper
{
    public static uint GetEntityId(this ServerCallContext context)
        => EntityId.Parser.ParseFrom(context.RequestHeaders.GetValueBytes("eid-bin")).Id;

    public static Entity GetEntity(this ServerCallContext context)
    {
        var id = context.GetEntityId();
        return Server.This.Domain.SearchEntity(context.GetEntityId()) as Entity ?? 
               throw new Exception("Can not find entity #{id}");
    }
}