namespace LocalMod.NetAbstractions;

public enum NetMethodCaller : byte
{
    /// <summary>
    /// A method that can be called by the server
    /// </summary>
    ServerCaller = 0,
    /// <summary>
    /// A method that can be called by the client 
    /// </summary>
    ClientCaller = 1,
}
