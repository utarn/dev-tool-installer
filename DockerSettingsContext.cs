using System.Text.Json.Serialization;

namespace DevToolInstaller;

[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(double))]
internal partial class DockerSettingsContext : JsonSerializerContext
{
}