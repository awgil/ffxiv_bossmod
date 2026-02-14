using System.Text.Json;

namespace BossMod.Interfaces;

public interface ISerializer
{
    public JsonSerializerOptions Options { get; }
    public string ToJSON<T>(T value);
    public T? FromJSON<T>(string json);
}
