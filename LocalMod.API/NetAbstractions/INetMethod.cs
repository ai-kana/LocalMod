namespace LocalMod.API.NetAbstractions;

public interface INetMethod
{
    /// <summary>
    /// Who can call this net method
    /// </summary>
    public NetMethodCaller AllowedCaller {get;}

    /// <summary>
    /// Unique method id
    /// </summary>
    public uint NetMethodId {get; set;}

    /// <summary>
    /// Delay between allowed method calls in seconds
    /// Zero is no delay
    /// </summary>
    public float RateLimit {get;}

    /// <summary>
    /// Called when the method is invoked
    /// </summary>
    public void ReceiveInvoke(InvocationData data);
}
