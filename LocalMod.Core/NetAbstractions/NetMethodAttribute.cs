namespace LocalMod.Core.NetAbstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NetMethodAttribute : Attribute
{
    public readonly NetMethodCaller AllowedCaller;
    public readonly string WriteMethodName;
    public readonly string ReadMethodName;
    public readonly string Name;

    public NetMethodAttribute(NetMethodCaller caller, string name, string read, string write)
    {
        AllowedCaller = caller;
        WriteMethodName = write;
        ReadMethodName = read;
        Name = name;
    }
}
