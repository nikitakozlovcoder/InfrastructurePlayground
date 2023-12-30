using System.Diagnostics;

namespace Telemetry;

public class TelemetryApp : ITelemetryApp
{
    public ActivitySource Source { get; }
    
    public TelemetryApp(string name)
    {
        Source = new ActivitySource(name);
    }
}