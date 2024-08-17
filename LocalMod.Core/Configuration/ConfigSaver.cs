using Newtonsoft.Json;

namespace LocalMod.Core.Configuration;

public static class ConfigSaver
{
    private const string ConfigPath = LocalModEntry.LocalModDirectory + "/Config/";
    private const string PathFormat = ConfigPath + "{0}.json" ;

    static ConfigSaver()
    {
        Directory.CreateDirectory(ConfigPath);
    }

    public static async Task<T> Load<T>(string name) where T : new()
    {
        string path = string.Format(PathFormat, name);
        if (!File.Exists(path))
        {
            return new();
        }

        string json;
        using (StreamReader reader = new(File.OpenRead(path)))
        {
            json = await reader.ReadToEndAsync();
        }

        return JsonConvert.DeserializeObject<T>(json) ?? new();
    }

    public static async Task Save(object config, string name)
    {
        string data = JsonConvert.SerializeObject(config, Formatting.Indented);
        string path = string.Format(PathFormat, name);
        await using (StreamWriter writer = new(File.Create(path)))
        {
            await writer.WriteAsync(data);
        }
    }
}
