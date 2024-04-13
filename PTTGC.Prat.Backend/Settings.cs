namespace PTTGC.Prat.Backend;

public class Settings
{
    public static Settings Instance { get; set; } = new();

    /// <summary>
    /// GCP Project Id
    /// </summary>
    public string GCPProjectId { get; set; }

    /// <summary>
    /// Gemini Model to use
    /// </summary>
    public string GeminiModel { get; set; }

    /// <summary>
    /// Embedding Model to use
    /// </summary>
    public string EmbeddingModel { get; set; }
}
