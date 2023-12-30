using System.Diagnostics;

namespace Telemetry;

public interface ITelemetryApp
{
    public ActivitySource Source { get; }
}