using System.Reflection;
using LocalMod.API.Reflection;
using SDG.Unturned;

namespace LocalMod.Core.NetAbstractions;

internal static class ServerMethodInfoFields
{
    public readonly static FieldInfo DeclaringTypeField 
        = typeof(ServerMethodInfo).GetInstanceField("declaringType")
        ?? throw new("Failed to find ServerMethod field: declaringType");

    public readonly static FieldInfo NameField 
        = typeof(ServerMethodInfo).GetInstanceField("name")
        ?? throw new("Failed to find ServerMethod field: name");

    public readonly static FieldInfo DebugNameField 
        = typeof(ServerMethodInfo).GetInstanceField("debugName")
        ?? throw new("Failed to find ServerMethod field: debugName");

    public readonly static FieldInfo CustomAttributeField 
        = typeof(ServerMethodInfo).GetInstanceField("customAttribute")
        ?? throw new("Failed to find ServerMethod field: customAttribute");

    public readonly static FieldInfo ReadMethodField 
        = typeof(ServerMethodInfo).GetInstanceField("readMethod")
        ?? throw new("Failed to find ServerMethod field: readMethod");

    public readonly static FieldInfo WriteMethodInfoField 
        = typeof(ServerMethodInfo).GetInstanceField("writeMethodInfo")
        ?? throw new("Failed to find ServerMethod field: writeMethodInfo");

    public readonly static FieldInfo MethodIndexField 
        = typeof(ServerMethodInfo).GetInstanceField("methodIndex")
        ?? throw new("Failed to find ServerMethod field: methodIndex");

    public readonly static FieldInfo RateLimitIndexField 
        = typeof(ServerMethodInfo).GetInstanceField("rateLimitIndex")
        ?? throw new("Failed to find ServerMethod field: rateLimitIndex");
}

