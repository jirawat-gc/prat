using HandlebarsDotNet;
using HandlebarsDotNet.Extension.NewtonsoftJson;
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Common;
using PTTGC.Prat.Common.Requests;
using System.Diagnostics;

using CompiledTemplate = HandlebarsDotNet.HandlebarsTemplate<object, object>;

namespace PTTGC.Prat.Backend.Domains;

public static class VertexAIDomain
{
    private static IHandlebars? templateEngine;

    static VertexAIDomain()
    {
        templateEngine = Handlebars.Create();
        templateEngine.Configuration.UseNewtonsoftJson();
    }

    /// <summary>
    /// Load prompt from Google Cloud Storage Bucket
    /// </summary>
    /// <returns></returns>
    public static async Task<Dictionary<string, CompiledTemplate>> LoadPrompts()
    {
        // in first version - use from memory, make it configurable later
        var toReturn = new Dictionary<string, string>();

        // load here
        toReturn[""] = "";

        // compile templates

        return toReturn.Select( tpl =>
            new {
                Key = tpl.Key,
                Template = templateEngine!.Compile(tpl.Value)
            }).ToDictionary( item => item.Key, item => item.Template);
    }

    private static Dictionary<string, CompiledTemplate> _Prompts = new();

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

        // call vertex ai

        return string.Empty;
    }

    /// <summary>
    /// Gets Embedding from given content
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task<double[]> GetEmbeddings( string content )
    {
        // call vertex ai to get embeddings

        return new double[0];
    }
}
