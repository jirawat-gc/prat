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

    /// <summary>
    /// Workspace Bucket Name
    /// </summary>
    public string WorkspaceBucketName { get; set; }

    /// <summary>
    /// Workspace Bucket Name
    /// </summary>
    public string PublicConfigBucketName { get; set; }

    /// <summary>
    /// Workspace Bucket Name
    /// </summary>
    public string PublicConfigBucketSignerCredential { get; set; }
}
