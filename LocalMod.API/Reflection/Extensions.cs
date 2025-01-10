using System.Reflection;

namespace LocalMod.API.Reflection;

public static class FieldInfoExtensions
{
    public static object GetValue(this FieldInfo info, object? instance = null)
    {
        return info.GetValue(instance);
    }

    public static T GetValue<T>(this FieldInfo info, object? instance = null)
    {
        return (T)info.GetValue(instance);
    }
}

public static class TypeExtensions
{
    private const BindingFlags AccessFlags = BindingFlags.NonPublic | BindingFlags.Public;
    public static FieldInfo GetInstanceField(this Type type, string name)
    {
        return type.GetField(name, BindingFlags.Instance | AccessFlags);
    }

    public static FieldInfo GetStaticField(this Type type, string name)
    {
        return type.GetField(name, BindingFlags.Static | AccessFlags);
    }

    public static MethodInfo GetInstanceMethod(this Type type, string name)
    {
        return type.GetMethod(name, BindingFlags.Instance | AccessFlags);
    }

    public static MethodInfo GetStaticMethod(this Type type, string name)
    {
        return type.GetMethod(name, BindingFlags.Static | AccessFlags);
    }
}

public static class AssemblyExtensions 
{
    public static Type GetNonPublicType(this Assembly assembly, string name)
    {
        return assembly.GetTypes().First(x => x.Name == name);
    }
}
