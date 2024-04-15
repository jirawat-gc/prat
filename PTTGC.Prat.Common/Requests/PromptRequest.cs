using Newtonsoft.Json.Linq;

namespace PTTGC.Prat.Common.Requests;

public class PromptRequest
{
    // ref: Defaults https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/gemini

    public float Temperature { get; set; } = 0.9f;

    public int MaxTokens { get; set; } = 8192;

    public float TopP { get; set; } = 1.0f;

    public int TopK { get; set; } = 32;

    public string PromptKey { get; set; }

    public JObject PromptContext { get; set; }

    /// <summary>
    /// Request to specified vertex ai region
    /// </summary>
    public string Region { get; set; }
}
