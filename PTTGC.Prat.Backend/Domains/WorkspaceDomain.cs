using Google;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Common;
using PTTGC.Prat.Core;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Text;

namespace PTTGC.Prat.Backend.Domains;

public static class WorkspaceDomain
{
    private static StorageClient _storage = StorageClient.Create();

    private static string GetWorkspaceObjectName(string ownerId, string workspaceId)
    {
        return $"{ownerId}/workspace-{workspaceId}.json";
    }

    private static string GetWorkspaceObjectName( Workspace ws )
    {
        return GetWorkspaceObjectName(ws.OwnerId, ws.Id.ToString());
    }

    /// <summary>
    /// Saves Workspace to GCS
    /// </summary>
    /// <param name="ws"></param>
    /// <returns></returns>
    public static async Task<Workspace> SaveWorkspace( Workspace ws )
    {
        // Create workspace in Google Cloud Storage

        // does not perform any analysis

        var json = JsonConvert.SerializeObject(ws);
        var bytes = Encoding.UTF8.GetBytes(json);

        // Upload JSON file to GCS bucket
        var obj = await _storage.UploadObjectAsync(
            Settings.Instance.WorkspaceBucketName,
            WorkspaceDomain.GetWorkspaceObjectName( ws ), "application/json",
            new MemoryStream(bytes));

        obj.Metadata = new Dictionary<string, string>();

        obj.Metadata["title"] = ws.InnovationTitle;
        obj.Metadata["description"] = ws.InnovationDescription;

        await _storage.UpdateObjectAsync(obj);

        return ws;
    }

    /// <summary>
    /// Loads Workspace from GCS
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="workspaceId"></param>
    /// <returns></returns>
    /// <exception cref="ExceptionWithErrorDetail"></exception>
    public static async Task<Workspace> LoadWorkspace( string ownerId, string workspaceId )
    {
        // look for workspace in Google Cloud Storage

        try
        {
            var objectName = WorkspaceDomain.GetWorkspaceObjectName(ownerId, workspaceId);
            var obj = await _storage.GetObjectAsync(Settings.Instance.WorkspaceBucketName, objectName);

            var buffer = new byte[(int)obj.Size];
            using var ms = new MemoryStream(buffer);
            await _storage.DownloadObjectAsync(Settings.Instance.WorkspaceBucketName, objectName, ms);

            var json = Encoding.UTF8.GetString(buffer);
            return JsonConvert.DeserializeObject<Workspace>(json);

        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new ExceptionWithErrorDetail(new ErrorDetail()
            {
                code = 404,
                message = ex.Message,
            });
        }
    }

    /// <summary>
    /// Lists all object
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public static async Task<List<Util.TitleDescription>> ListWorkspaces( string ownerId )
    {
        var options = new ListObjectsOptions { Delimiter = "/" };
        var listOperation = _storage.ListObjectsAsync(
            Settings.Instance.WorkspaceBucketName,
            $"{ownerId}/", options);

        var result = new List<Util.TitleDescription>();

        await foreach (var obj in listOperation)
        {
            result.Add(new Util.TitleDescription(obj.Metadata.Get("title"), obj.Metadata.Get("description")));
        }

        return result;
    }
}
