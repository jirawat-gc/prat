using PTTGC.Prat.Common;
using PTTGC.Prat.Core;

namespace PTTGC.Prat.Backend.Domains;

public static class PatentClusterDomain
{
    /// <summary>
    /// Gets the local cluster of given innovation
    /// </summary>
    /// <param name="innovationDescription"></param>
    /// <param name="innovationFlags"></param>
    /// <returns></returns>
    public static async Task<PatentCluster> GetLocalCluster(double[] embeddingVector, IDictionary<string, bool> innovationFlags)
    {
        // run the Vertex AI Custom DBSCAN model to find our cluster

        // perform similarity 

        // return the matching cluster

        return new PatentCluster();
    }

    /// <summary>
    /// Gets list of cluster for rendering the explore scatter plot
    /// </summary>
    /// <returns></returns>
    public static async Task<List<PatentCluster>> GetClusters()
    {
        // use bigquery to find all clusters but not member of the clusters

        // return the clusters and the centoid

        return new List<PatentCluster>();
    }

    /// <summary>
    /// Gets all patent details from given cluster label
    /// </summary>
    /// <param name="clusterLabel"></param>
    /// <returns></returns>
    public static async Task<List<Patent>> GetClusterMember( string clusterLabel)
    {
        // get the row of that patent in bigquery

        // use bigquery to find all patents from the list

        return new List<Patent>();
    }
}
