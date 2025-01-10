using System.Reflection;
using Autofac;
using LocalMod.API.IoC;
using Microsoft.Extensions.DependencyInjection;

namespace LocalMod.Core.IoC;

internal static class ServiceLoader
{
    public static void LoadServices(Assembly assembly, ContainerBuilder builder)
    {
        foreach (Type type in assembly.GetTypes())
        {
            ServiceAttribute attribute = type.GetCustomAttribute<ServiceAttribute>();
            if (attribute == null)
            {
                continue;
            }

            Type serviceType = attribute.ServiceType ?? type;

            if (type.IsGenericType)
            {
                RegisterGeneric(type, serviceType, attribute.LifeTime, builder);
            }
            else
            {
                Register(type, serviceType, attribute.LifeTime, builder);
            }
        }
    }

    private static void RegisterGeneric(Type type, Type serviceType, ServiceLifetime lifetime, ContainerBuilder builder)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                builder.RegisterGeneric(type).As(serviceType).SingleInstance();
                break;
            case ServiceLifetime.Scoped:
                builder.RegisterGeneric(type).As(serviceType).OwnedByLifetimeScope();
                break;
        }
    }

    private static void Register(Type type, Type serviceType, ServiceLifetime lifetime, ContainerBuilder builder)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                builder.RegisterType(type).As(serviceType).SingleInstance();
                break;
            case ServiceLifetime.Scoped:
                builder.RegisterType(type).As(serviceType).OwnedByLifetimeScope();
                break;
        }
    }
}
