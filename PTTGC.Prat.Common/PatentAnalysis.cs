using PTTGC.Prat.Core;

namespace PTTGC.Prat.Common;

public class PatentAnalysis
{
    /// <summary>
    /// List of Standalone Claims found in patent
    /// </summary>
    public string StandaloneClaims { get; set; }

    /// <summary>
    /// List of Material Attributes found in patent
    /// </summary>
    public List<MaterialAttribute> MaterialAttributes { get; set; } = new();

    /// <summary>
    /// Application as mentioned in patent claim
    /// </summary>
    public string Application { get; set; }

    /// <summary>
    /// Similarity score when comparing all flags
    /// </summary>
    public double FlagsSimilarity { get; set; }

    /// <summary>
    /// Similarity score when comparing text embedding
    /// </summary>
    public double TextEmbeddingSimilarity { get; set; }

}
