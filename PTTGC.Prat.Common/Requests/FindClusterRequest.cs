namespace PTTGC.Prat.Common.Requests;

public class FindClusterRequest
{
    public double[] EmbeddingVector {  get; set; }

    public double[] FeatureFlags { get; set; }

    public FindClusterRequest(double[] summaryEmbeddingVector, Dictionary<string, bool> flags)
    {
        this.EmbeddingVector = summaryEmbeddingVector;
        this.FeatureFlags = FindClusterRequest.BuildFeatureFlagArray(flags).ToArray();
    }

    /// <summary>
    /// Create list of flag with same "boosting" as in 04 - DBSCAN cluster from Feature and Embedding Notebook
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static IEnumerable<double> BuildFeatureFlagArray(Dictionary<string, bool> flags)
    {
        // columns list, as in Python Notebook:
        var featureColumnsOrder = "chemical,polymer,home_chemical,industrial_chemical,argriculture,automotive,bottle_caps,bottle,paint,coating,home_appliances,expoy_composite,packaging,large_blow,non_woven,pipe,pipe_fittings,wire,has_chemical_process,has_manufacturing_process"
                                        .Split(',');

        // follow the "Boosting" to seprate chemical/polymer cluster
        foreach (var item in featureColumnsOrder)
        {
            var value = flags.Get(item) ? 1 : 0;

            if (item == "chemical")
            {
                yield return (value * -1.5);
            }
            else if (item == "polymer")
            {
                yield return (value * 1.5);
            }
            else
            {
                yield return value;
            }
        }
    }
}
