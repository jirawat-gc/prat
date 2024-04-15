using Google.Api.Gax.Grpc;
using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.AIPlatform.V1;
using HandlebarsDotNet;
using HandlebarsDotNet.Extension.NewtonsoftJson;
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Common;
using PTTGC.Prat.Common.Requests;
using System.Diagnostics;
using System.Text;
using CompiledTemplate = HandlebarsDotNet.HandlebarsTemplate<object, object>;
using Value = Google.Protobuf.WellKnownTypes.Value;

namespace PTTGC.Prat.Backend.Domains;

public static partial class VertexAIDomain
{
    private static IHandlebars? templateEngine;

    private static List<PredictionServiceClient> _Clients;
    private static string[] _Regions;

    static VertexAIDomain()
    {
        templateEngine = Handlebars.Create();
        templateEngine.Configuration.UseNewtonsoftJson();

        _Regions = new string[]
        {
            "us-central1",
            "us-west4",
            "us-east4",
            "us-west1",
            "asia-northeast3",
            //"asia-southeast1", -- used by BigQuery
            "asia-northeast1",
        };

        _Clients = _Regions.Select(r =>
        {
            return new PredictionServiceClientBuilder
            {
                Endpoint = $"{r}-aiplatform.googleapis.com"
            }.Build();

        }).ToList();
    }

    /// <summary>
    /// Load prompt from Google Cloud Storage Bucket
    /// </summary>
    /// <returns></returns>
    public static async Task<Dictionary<string, CompiledTemplate>> LoadPrompts()
    {
        // in first version - use from memory, make it configurable later
        var toReturn = new Dictionary<string, string>();

        // in real code, we will load prompts from configuration file in GCS
        // but for POC the prompt is hard-coded to reduce moving parts
        AddPromptsPOC(toReturn);

        // compile templates

        return toReturn.Select( tpl =>
            new {
                Key = tpl.Key,
                Template = templateEngine!.Compile(tpl.Value)
            }).ToDictionary( item => item.Key, item => item.Template);
    }

    private static Dictionary<string, CompiledTemplate> _Prompts = new();

    private static long _RegionRotation = 0;

    private static int RotateRegion()
    {
        return (int)(Interlocked.Increment(ref _RegionRotation) % _Clients.Count);
    }

    /// <summary>
    /// Gets Completion from Vertex AI
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    /// <exception cref="ExceptionWithErrorDetail"></exception>
    public static async Task<string> GetCompletion( PromptRequest req )
    {
        if (_Prompts.Count == 0)
        {
            _Prompts = await VertexAIDomain.LoadPrompts();
        }

        CompiledTemplate template;
        if (_Prompts.TryGetValue( req.PromptKey, out template) == false)
        {
            throw new ExceptionWithErrorDetail(new ErrorDetail()
            {
                code = 400,
                message = "Prompt is not found"
            });
        }

        // render the template

        var finalPrompt = template(req.PromptContext);

        // ref: https://github.com/GoogleCloudPlatform/dotnet-docs-samples/blob/main/aiplatform/api/AIPlatform.Samples/GeminiQuickstart.cs

        // call vertex ai
        var regionIndex = VertexAIDomain.RotateRegion();
        var client = _Clients[regionIndex];

        // Initialize request argument(s)
        var content = new Content
        {
            Role = "USER"
        };
        content.Parts.AddRange(new List<Part>()
        {
            new() {
                Text = finalPrompt
            }
        });

        var region = req.Region ?? _Regions[regionIndex];

        var generateContentRequest = new GenerateContentRequest
        {
            Model = $"projects/{Settings.Instance.GCPProjectId}/locations/{region}/publishers/google/models/{Settings.Instance.GeminiModel}",
            GenerationConfig = new GenerationConfig
            {
                Temperature = req.Temperature,
                TopP = req.TopP,
                TopK = req.TopK,
                MaxOutputTokens = req.MaxTokens
            }
        };
        generateContentRequest.Contents.Add(content);

        // Make the request, returning a streaming response
        using PredictionServiceClient.StreamGenerateContentStream response = client.StreamGenerateContent(generateContentRequest);

        StringBuilder fullText = new();

        AsyncResponseStream<GenerateContentResponse> responseStream = response.GetResponseStream();
        await foreach (GenerateContentResponse responseItem in responseStream)
        {
            fullText.Append(responseItem.Candidates[0].Content.Parts[0].Text);
        }

        return fullText.ToString();
    }

    /// <summary>
    /// Gets Embedding from given content
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task<double[]> GetEmbeddings( string content )
    {
        // call vertex ai to get embeddings

        // Ref: https://cloud.google.com/vertex-ai/docs/samples/aiplatform-sdk-embedding

        var regionIndex = VertexAIDomain.RotateRegion();
        var client = _Clients[regionIndex];

        // Configure the parent resource.
        var endpoint = EndpointName.FromProjectLocationPublisherModel(
            Settings.Instance.GCPProjectId,
            _Regions[regionIndex], "google", Settings.Instance.EmbeddingModel);

        // Initialize request argument(s).
        var instances = new List<Value>
        {
            Value.ForStruct(new()
            {
                Fields =
                {
                    ["content"] = Value.ForString(content),
                }
            })
        };

        // Make the request.
        var response = client.Predict(endpoint, instances, null);

        // Parse and return the embedding vector count.
        var values = response.Predictions.First().StructValue.Fields["embeddings"].StructValue.Fields["values"].ListValue.Values;

        return values.Select(v => v.NumberValue).ToArray();
    }
}
