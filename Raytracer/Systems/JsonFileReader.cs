using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;

namespace Raytracer.Systems;

public class JsonFileReader
{
    private static readonly JsonSerializerOptions _defaultOptions = CreateDefaultOptions();

    private static JsonSerializerOptions CreateDefaultOptions() {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new Vector3dJsonConverter());
        return options;
    }

    public static T? Deserialize<T>(string configFilePath, JsonSerializerOptions? options = null)
    {
        try
        {
            string json = File.ReadAllText(configFilePath);
            return JsonSerializer.Deserialize<T>(json, options ?? _defaultOptions);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"Failed to read config file '{configFilePath}': {ex.Message}");
            return default;
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"Failed to parse config file '{configFilePath}': {ex.Message}");
            return default;
        }
    }
}

public class Vector3dJsonConverter : JsonConverter<Vector3d>
{
    public override Vector3d Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected object for Vector3d.");

        double? x = null;
        double? y = null;
        double? z = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (x is null || y is null || z is null)
                    throw new JsonException("Vector3d must have X, Y, and Z.");

                return new Vector3d(x.Value, y.Value, z.Value);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected property name.");

            string name = reader.GetString()!;
            reader.Read();

            switch (name)
            {
                case "X":
                case "x":
                    x = reader.GetDouble();
                    break;
                case "Y":
                case "y":
                    y = reader.GetDouble();
                    break;
                case "Z":
                case "z":
                    z = reader.GetDouble();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException("Invalid Vector3d JSON.");
    }

    public override void Write(Utf8JsonWriter writer, Vector3d value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Z", value.Z);
        writer.WriteEndObject();
    }
}