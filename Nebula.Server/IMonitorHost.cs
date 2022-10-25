using Nebula.Protocols;

namespace Nebula.Server;

public interface IMonitorHost
{
    PropertyList ListProperties();

    PropertyContent GetPropertyData(PropertyReadingRequest request);

    void SetPropertyData(PropertyContent request);

    void StartMonitor(PropertyMonitorRequest request);

    void StopMonitor(PropertyId request);
}