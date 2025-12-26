using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharedKernel.Helper.Interfaces;

namespace SharedKernel.Helper;

public class SerializerService : ISerializerService
{
    private readonly JsonSerializerOptions _options;

    public SerializerService()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        _options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public T? Deserialize<T>(string text)
    {
        return JsonSerializer.Deserialize<T>(text, _options);
    }

    public string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, _options);
    }

    public string Serialize<T>(T obj, Type type)
    {
        return JsonSerializer.Serialize(obj, type, _options);
    }
}
