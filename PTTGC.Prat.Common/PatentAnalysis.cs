using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Core;
using System.Collections.Concurrent;

namespace PTTGC.Prat.Common;

public class PatentAnalysis
{
    public class AttributeAnalyis
    {
        public string attribute { get; set; }
    }

    public class Claim
    {
        public int index { get; set; }
        public string claim { get; set; }
        public List<int> citations { get; set; }
        public string revision { get; set; }
    }

    public class TestResult
    {
        public string attribute { get; set; }
        public string value_lower_bound { get; set; }
        public string value_upper_bound { get; set; }
        public string unit { get; set; }

        public MaterialAttribute MatchingInnovationAttribute { get; set; }
    }
    /// <summary>
    /// List of Standalone Claims found in patent
    /// </summary>
    public List<Claim> Claims { get; set; } = new();

    /// <summary>
    /// List of Material Attributes found in patent
    /// </summary>
    public List<TestResult> TestResults { get; set; } = new();

    public List<AttributeAnalyis> SameAttributes { get; set; } = new ();

    public List<AttributeAnalyis> MissingAttributes { get; set; } = new();

    public List<AttributeAnalyis> SameApplications { get; set; } = new();

    /// <summary>
    /// Similarity score when comparing all flags
    /// </summary>
    public double? FlagsSimilarity { get; set; }

    /// <summary>
    /// Similarity score when comparing text embedding
    /// </summary>
    public double? TextEmbeddingSimilarity { get; set; }

    /// <summary>
    /// Prompt Response when performing analysis in each phases
    /// </summary>
    public ConcurrentDictionary<string, string> PromptResponses { get; set; } = new();

    /// <summary>
    /// Prompt Output after analysis
    /// </summary>
    public Dictionary<string, JObject> PromptOutput { get; set; } = new();

    /// <summary>
    /// Whether analysis has been ran
    /// </summary>
    public bool IsAnalysisCompleted { get; set; }

    /// <summary>
    /// Whether analysis is running
    /// </summary>
    public bool IsAnalyzing { get; set; }

    public void PopulateAnalysis()
    {
        foreach (var item in this.PromptResponses)
        {
            try
            {
                var response = item.Value;
                var responseJO = response.FixJson();

                this.PromptOutput[item.Key] = responseJO;
            }
            catch (Exception)
            {
            }
        }
    }
}
