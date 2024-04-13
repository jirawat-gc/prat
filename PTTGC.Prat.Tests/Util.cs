using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PTTGC.Prat.Backend;
using PTTGC.Prat.Common;

namespace PTTGC.Prat.Tests;

internal class Util
{
    public static Settings InitConfiguration()
    {
        var config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        config.Bind(Settings.Instance);

        return Settings.Instance;
    }

    static Util()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Converters = new[] { new VectorEmbedding.VectorEmbeddingConverter() }
        };
    }
}
