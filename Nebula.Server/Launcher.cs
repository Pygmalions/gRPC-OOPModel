using System.CommandLine;
using System.Net.NetworkInformation;
using System.Reflection;

namespace Nebula.Server;

public static class Launcher
{
    public static async Task Main(string[] arguments)
    {
        var commandRoot = new RootCommand(
            $"Nebula.Server {Assembly.GetExecutingAssembly().GetName().Version!}");
        
        var optionNexus = new Option<string>("--nexus", "Uri of the Nebula Nexus.")
        {
            IsRequired = true
        };
        optionNexus.AddAlias("-n");
        commandRoot.AddOption(optionNexus);

        var optionAssembly = new Option<string>("--assembly", "Path of the service assembly to run.")
        {
            IsRequired = true
        };
        optionAssembly.AddAlias("-a");
        commandRoot.AddOption(optionAssembly);
        
        var optionClass = new Option<string>("--class", "Name of the class for this server to run.")
        {
            IsRequired = true
        };
        optionClass.AddAlias("-c");
        commandRoot.AddOption(optionClass);
        
        var optionEnvironment = new Option<string?>("--environment", ()=> null,
            "Name of the environment.");
        optionEnvironment.AddAlias("-e");
        commandRoot.AddOption(optionEnvironment);
        
        var optionPort = new Option<int>("--port", () =>
        {
            var ipProperty = IPGlobalProperties.GetIPGlobalProperties();
            var usedPorts = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
                .Select(endpoint => endpoint.Port).ToHashSet();
            var port = 9001;
            while (usedPorts.Contains(port))
                port++;
            return port;
        }, "Port for this server to use.");
        optionPort.AddAlias("-p");
        commandRoot.AddOption(optionPort);
        
        commandRoot.SetHandler((nexus, port, assemblyPath,className, environment) =>
            {
                if (Assembly.LoadFrom(assemblyPath).GetType(className) is not { } service)
                    throw new Exception("Failed to load service.");
                Server.This = new Server(new Uri(nexus), port, service, environment);
                Server.This.Start().Wait();
            }, 
            optionNexus, optionPort, optionAssembly, optionClass, optionEnvironment);

        await commandRoot.InvokeAsync(arguments);
    }
}