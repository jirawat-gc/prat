using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PTTGC.Prat.Common;
using PTTGC.Prat.Core;
using PTTGC.Prat.Common.Requests;
using PTTGC.Prat.Common.Response;
using System.IO.Compression;

namespace PTTGC.Prat.Web;

public class PratBackend
{
    public static PratBackend Default { get; private set; } = new();

    public string BaseAddress { get; set; }

    public string GCSBaseAddress { get; set; }

    public string AccessToken { get; set; }

    public string SessionId { get; set; } = Guid.NewGuid().ToString();

    public string AppId { get; set; } = "PRAT-Beta";

    /// <summary>
    /// Initializes HTTP Client for interaction with Prat Backend
    /// </summary>
    /// <returns></returns>
    private HttpClient GetHttpClient(string appId, string sessionId)
    {
        var client = new HttpClient();

#if DEBUG
        client.BaseAddress = new Uri("http://localhost:7253/");
#else
        client.BaseAddress = new Uri(this.BaseAddress);
#endif

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {this.AccessToken}");
        client.DefaultRequestHeaders.Add("X-GC-AppId", appId);
        client.DefaultRequestHeaders.Add("X-GC-SessionId", sessionId);
        client.DefaultRequestHeaders.Add("X-GC-RequestTimeStamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

        return client;
    }

    /// <summary>
    /// Gets JSON from given URL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<JObject> Get(string pathAndQuery, string appId, string sessionId)
    {
        using var c = this.GetHttpClient(appId, sessionId);

        var s = await c.GetStreamAsync(pathAndQuery);

        using var sr = new StreamReader(s);
        using var jtr = new JsonTextReader(sr);

        var jo = await JObject.LoadAsync(jtr);

        await s.DisposeAsync();

        return jo;
    }

    /// <summary>
    /// Gets JSON from given URL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<JToken> GetJsonFromGCS(string url, bool compressed = false)
    {
        using var c = new HttpClient();
        var s = await c.GetStreamAsync( $"{this.GCSBaseAddress}{url}");

        GZipStream gz = null;
        if (compressed)
        {
            gz = new GZipStream(s, CompressionMode.Decompress);
            s = gz;
        }

        using var sr = new StreamReader(s);
        using var jtr = new JsonTextReader(sr);

        var jt = await JToken.LoadAsync(jtr);

        if (gz != null)
        {
            await gz.DisposeAsync();
        }
        await s.DisposeAsync();

        return jt;
    }

    /// <summary>
    /// Gets JSON from given URL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<object> PopulateFromGCS(string url, object target, bool compressed = false)
    {
        using var c = new HttpClient();
        var s = await c.GetStreamAsync($"{this.GCSBaseAddress}{url}");

        GZipStream gz = null;
        if (compressed)
        {
            gz = new GZipStream(s, CompressionMode.Decompress);
            s = gz;
        }

        using var sr = new StreamReader(s);
        var json = await sr.ReadToEndAsync();

        if (gz != null)
        {
            await gz.DisposeAsync();
        }
        await s.DisposeAsync();

        JsonConvert.PopulateObject(json, target);

        return target;
    }

    /// <summary>
    /// Perform HTTP Post with JSON and read response as JSON
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<JObject> PostJsonAsync<T>(string url, T data, string appId, string sessionId)
    {
        var client = this.GetHttpClient(appId, sessionId);

        var json = JsonConvert.SerializeObject(data);
        var requestContent = new StringContent(json, null, "application/json");
        requestContent.Headers.ContentType.CharSet = "";

        try
        {
            var response = await client.PostAsync(url, requestContent);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new ExceptionWithErrorDetail(JsonConvert.DeserializeObject<ErrorDetail>(responseJson));
            }

            if (response.IsSuccessStatusCode == false)
            {
                throw new InvalidOperationException("Not Successful Status Code");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return new JObject();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return new JObject();
            }

            return JObject.Parse(responseJson);

        }
        catch (ExceptionWithErrorDetail ex)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("Could not complete request", ex);
        }
    }

    /// <summary>
    /// Submit workspace to be saved on GCP
    /// </summary>
    /// <param name="w"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<Workspace> SubmitWorkspace( Workspace w )
    {
        var jo = await this.PostJsonAsync("workspace", w, this.AppId, this.SessionId);
        return jo["data"].ToObject<Workspace>();
    }

    /// <summary>
    /// Load Workspace from GCS
    /// </summary>
    /// <param name="w"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<Workspace> LoadWorkspace(string workspaceId)
    {
        var jo = await this.Get($"workspace/{workspaceId}", this.AppId, this.SessionId);
        return jo["data"].ToObject<Workspace>();
    }

    /// <summary>
    /// List Cluster Member
    /// </summary>
    /// <param name="w"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<List<Patent>> FindSimilarPatent(SimilaritySearchRequest req)
    {
        var jo = await this.PostJsonAsync($"similaritysearch", req, this.AppId, this.SessionId);

        return jo["data"].ToObject<List<Patent>>();
    }

    /// <summary>
    /// Send Prompt to Vertex AI
    /// </summary>
    /// <param name="w"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<string> Prompt(PromptRequest req)
    {
        var jo = await this.PostJsonAsync("prompt", req, this.AppId, this.SessionId);
        return jo["data"].ToString();
    }

    /// <summary>
    /// Get Embeddings from Vertex AI
    /// </summary>
    /// <param name="w"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<VectorEmbedding> Embeddings(EmbeddingRequest req)
    {
        var jo = await this.PostJsonAsync("embeddings", req, this.AppId, this.SessionId);
        var data = jo["data"].ToObject<EmbeddingResponse>();
        return new VectorEmbedding(data.VectorBase64);
    }
}
