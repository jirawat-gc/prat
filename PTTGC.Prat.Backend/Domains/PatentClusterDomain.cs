using Google.Apis.Auth.OAuth2;
using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.BigQuery.V2;
using Google.Cloud.Storage.V1;
using PTTGC.Prat.Common;
using PTTGC.Prat.Common.Response;
using PTTGC.Prat.Core;
using System.Security.AccessControl;
using System.Text;

namespace PTTGC.Prat.Backend.Domains;

public static class PatentClusterDomain
{
    private static UrlSigner _Signer;
    private static StorageClient _GCS;

    static PatentClusterDomain()
    {
        _GCS = StorageClient.Create();

        var key = Encoding.UTF8.GetString(Convert.FromBase64String(Settings.Instance.PublicConfigBucketSignerCredential));
        var signerClient = StorageClient.Create(GoogleCredential.FromJson(key));
        _Signer = signerClient.CreateUrlSigner();
    }

    /// <summary>
    /// Gets the local cluster of given innovation
    /// </summary>
    /// <param name="innovationDescription"></param>
    /// <param name="innovationFlags"></param>
    /// <returns></returns>
    public static async Task<PatentCluster> GetLocalCluster(double[] embeddingVector, double[] innovationFlags)
    {
        
        // scale the data to 0/1 based on

        // run the Vertex AI Custom DBSCAN model to find our cluster

        // perform similarity 

        // return the matching cluster

        return new PatentCluster();
    }

    private static PatentClusterSignedUrlsData _SignedUrlCache = new();

    public static async Task<PatentClusterSignedUrlsData> GetPatentClusterSignedUrls()
    {
        if (_SignedUrlCache.Expiry > DateTimeOffset.UtcNow)
        {
            return _SignedUrlCache;
        }

        _SignedUrlCache = await PatentClusterDomain.GetPatentClusterSignedUrlsInternal();
        return _SignedUrlCache;
    }

    /// <summary>
    /// Gets list of cluster URL for rendering the explore scatter plot
    /// </summary>
    /// <returns></returns>
    private static async Task<PatentClusterSignedUrlsData> GetPatentClusterSignedUrlsInternal()
    {   
        // Does not need to query, the data is ready in JSON file on GCS
        // We return signed URL in a single call so client can handle all by itself

        var options = new ListObjectsOptions { Delimiter = "/" };
        var listOperation = _GCS.ListObjectsAsync(
            Settings.Instance.PublicConfigBucketName,
            "patentcluster", options);

        var result = new PatentClusterSignedUrlsData();

        await foreach (var obj in listOperation)
        {
            var signed = await _Signer.SignAsync(
                Settings.Instance.PublicConfigBucketName,
                obj.Name,
                TimeSpan.FromHours(4),
                HttpMethod.Get);

            result.Urls.Add(signed);
        }

        result.Expiry = DateTimeOffset.UtcNow.AddHours(3.8);

        return result;
    }

}
