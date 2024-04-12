using PTTGC.Prat.Core;

namespace PTTGC.Prat.Backend.Domains;

public static class SimilaritySearchDomain
{
    public static async Task<List<Patent>> FindMatches(double[] embeddingVector )
    {
        // use innovation summary to create vector embedding

        // use BigQuery to find the matches

        // return matches

        return new List<Patent>();
    }
}
