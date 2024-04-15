using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Core;
using System.Collections.Concurrent;

namespace PTTGC.Prat.Common;

public class PatentAnalysis
{
    public class StandaloneClaim
    {
        public string Claim { get; set; }

        [JsonProperty("point_index")]
        public string PointIndex { get; set; }

        public string Revision { get; set; }
    }

    /// <summary>
    /// List of Standalone Claims found in patent
    /// </summary>
    public List<StandaloneClaim> StandaloneClaims { get; set; } = new();

    /// <summary>
    /// List of Material Attributes found in patent
    /// </summary>
    public List<MaterialAttribute> MaterialAttributes { get; set; } = new();

    /// <summary>
    /// Application as mentioned in patent claim
    /// </summary>
    public string Application { get; set; }

    public bool? IsOlefins { get; set; }

    public bool? IsAromatics { get; set; }

    public bool? IsPolyolefins { get; set; }

    public bool? IsPolyethylene { get; set; }

    public string? PolymerBase { get; set; }

    public string? PolymerComonomer { get; set; }

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
    /// Whether analysis has been ran
    /// </summary>
    public bool IsAnalysisCompleted { get; set; }

    /// <summary>
    /// Whether analysis is running
    /// </summary>
    public bool IsAnalyzing { get; set; }

    public void PopulateAnalysis()
    {
        this.PopulatePolymerFeatures();
        this.PopulateStandaloneClaim();
        this.PopulateMaterialAttributes();
    }

    public void PopulateMaterialAttributes()
    {
        var response = this.PromptResponses.Get("EXTRACT_TEST_RESULT");
        var responseJO = response.FixJson();

        var list = responseJO["test_results"] as JArray;
        if (list == null)
        {
            return;
        }

        this.MaterialAttributes = new();
        foreach (var item in list)
        {
            var jo = item as JObject;
            this.MaterialAttributes.Add(new MaterialAttribute()
            {
                AttributeName = jo.TryGetString("test"),
                TestCondition = jo.TryGetString("condition"),
                LowerBound = jo.TryGetFloat("value_lower_bound"),
                UpperBound = jo.TryGetFloat("value_upper_bound"),
                MeasurementUnit = jo.TryGetString("unit"),
                FromRevision = jo.TryGetString("revision"),
            });
        }
    }

    public void PopulateStandaloneClaim()
    {
        var response = this.PromptResponses.Get("EXTRACT_MAIN_CLAIM");
        var responseJO = response.FixJson();

        var list = responseJO["standalone_claims"] as JArray;
        if (list == null)
        {
            return;
        }

        this.StandaloneClaims = new();
        foreach (var item in list)
        {
            var jo = item as JObject;
            this.StandaloneClaims.Add(jo.ToObject<StandaloneClaim>());
        }
    }

    public void PopulatePolymerFeatures()
    {
        var response = this.PromptResponses.Get("EXTRACT_POLYMER_FEATURE");
        var responseJO = response.FixJson();

        this.IsOlefins = responseJO.TryGetBool("is_olefins");
        this.IsAromatics = responseJO.TryGetBool("is_aromatics");
        this.IsPolyolefins = responseJO.TryGetBool("is_polyolefins");
        this.IsPolyethylene = responseJO.TryGetBool("is_polyethylene");

        this.PolymerBase = responseJO.TryGetString("base_of_polymer");
        this.PolymerComonomer = responseJO.TryGetString("comonomer");
        this.Application = responseJO.TryGetString("application");
    }
}
