using Microsoft.Extensions.DependencyInjection;

namespace LocalMod.API.IoC;

/// <summary>
/// Marks class as service to be added to container on load
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ServiceAttribute : Attribute
{
    public readonly ServiceLifetime LifeTime;
    public readonly Type? ServiceType;

    public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient, Type? serviceType = null)
    {
        LifeTime = lifetime;
        ServiceType = serviceType;
    }
}
