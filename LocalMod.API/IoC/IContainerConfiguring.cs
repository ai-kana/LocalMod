using Autofac;

namespace LocalMod.API.IoC;

public interface IContainerConfiguring
{
    public void OnConfiguring(ContainerBuilder builder);
}
