using Newtonsoft.Json.Linq;

namespace PTTGC.Prat.Common.Requests;

public class PromptRequest
{
    public double Temperature { get; set; }

    public double MaxTokens { get; set; }

    public double TopP { get; set; }

    public string PromptKey { get; set; }

    public JObject PromptContext { get; set; }
}
