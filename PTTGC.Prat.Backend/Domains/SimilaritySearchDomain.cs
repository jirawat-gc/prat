using Google.Cloud.BigQuery.V2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Common;
using PTTGC.Prat.Core;

namespace PTTGC.Prat.Backend.Domains;

public static class SimilaritySearchDomain
{
    public static async Task<List<Patent>> FindMatches(double[] embeddingVector )
    {
        var client = BigQueryClient.Create(Settings.Instance.GCPProjectId);

        var vector = JArray.FromObject(embeddingVector).ToString(Formatting.None);
        var topK = 100;

        // We optimize it by keeping Patents as separate JSON file in GCS
        // and use short-lived token to have client download it by themselves

        string query = $@"
        SELECT 
            base.ApplicationId,
            base.Title,
            base.Summary,
            distance
        FROM
            VECTOR_SEARCH(
            TABLE patents_processed.thailand_dip_3_embeddings,
            'SummaryEmbedding',
            (select {vector} as SummaryEmbedding),
                top_k => {topK},
                distance_type => 'COSINE',
                options => '{{""fraction_lists_to_search"": 0.005}}')

        WHERE
            distance < 0.3
            ";

        var toReturn = new List<Patent>();
        var results = await client.ExecuteQueryAsync(query, new BigQueryParameter[0]);

        // Process the results
        foreach (BigQueryRow row in results)
        {
            toReturn.Add(new Patent()
            {
                ApplicationId = row["ApplicationId"].ToString(),
                Title = row["Title"].ToString(),
                Summary = row["Summary"].ToString(),
                Analysis = new PatentAnalysis { 
                    TextEmbeddingSimilarity = (double)row["distance"]
                }
            });
        }

        return toReturn;
    }
}
