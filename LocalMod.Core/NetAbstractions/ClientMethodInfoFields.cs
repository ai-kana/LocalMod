using System.Reflection;
using LocalMod.API.Reflection;
using SDG.Unturned;

namespace LocalMod.Core.NetAbstractions;

internal static class ClientMethodInfoFields
{
    public readonly static FieldInfo DeclaringTypeField 
        = typeof(ClientMethodInfo).GetInstanceField("declaringType")
        ?? throw new("Failed to find ClientMethod: declaringType");

    public readonly static FieldInfo NameField 
        = typeof(ClientMethodInfo).GetInstanceField("name")
        ?? throw new("Failed to find ClientMethod field: name");

    public readonly static FieldInfo DebugNameField 
        = typeof(ClientMethodInfo).GetInstanceField("debugName")
        ?? throw new("Failed to find ClientMethod field: debugName");

    public readonly static FieldInfo CustomAttributeField 
        = typeof(ClientMethodInfo).GetInstanceField("customAttribute")
        ?? throw new("Failed to find ClientMethod field: customAttribute");

    public readonly static FieldInfo ReadMethodField 
        = typeof(ClientMethodInfo).GetInstanceField("readMethod")
        ?? throw new("Failed to find ClientMethod field: readMethod");

    public readonly static FieldInfo WriteMethodInfoField 
        = typeof(ClientMethodInfo).GetInstanceField("writeMethodInfo")
        ?? throw new("Failed to find ClientMethod field: writeMethodInfo");

    public readonly static FieldInfo MethodIndexField 
        = typeof(ClientMethodInfo).GetInstanceField("methodIndex")
        ?? throw new("Failed to find ClientMethod field: methodIndex");
}
