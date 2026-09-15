using System.Collections.Concurrent;

namespace Mdaresna.Platform.Worker;

internal sealed class WorkerReadinessState
{
    private readonly ConcurrentDictionary<string, bool> _components = new();

    public WorkerReadinessState(IConfiguration configuration)
    {
        RegisterIfEnabled("sms-log", configuration.GetValue<bool>("SmsLogConsumer:Enabled"));
        RegisterIfEnabled(
            "school-registration",
            configuration.GetValue<bool>("SchoolRegistrationConsumer:Enabled"));
    }

    public bool IsReady => _components.IsEmpty || _components.Values.All(value => value);

    public void MarkReady(string component) => Set(component, true);

    public void MarkNotReady(string component) => Set(component, false);

    private void RegisterIfEnabled(string component, bool enabled)
    {
        if (enabled)
        {
            _components.TryAdd(component, false);
        }
    }

    private void Set(string component, bool ready)
    {
        if (_components.ContainsKey(component))
        {
            _components[component] = ready;
        }
    }
}
