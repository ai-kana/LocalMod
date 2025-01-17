using System.Reflection;
using Cysharp.Threading.Tasks;

namespace LocalMod.Core.Manifest;

internal static class ManifestHelper
{
    public static async UniTask CopyToFile(Assembly assembly, string manifestPath, string realPath)
    {
        await using Stream? stream = assembly.GetManifestResourceStream(manifestPath);
        if (stream == null)
        {
            throw new ArgumentException("Invalid manifest path");
        }

        using StreamReader reader = new(stream);
        string buffer = await reader.ReadToEndAsync();

        await using StreamWriter writer = new(File.Open(realPath, FileMode.Create));
        await writer.WriteAsync(buffer);
    }
}
