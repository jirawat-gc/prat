using Newtonsoft.Json.Linq;
using PTTGC.Prat.Common;
using PTTGC.Prat.Core;
namespace PTTGC.Prat.Web;

public class ProjectData
{
    public static ProjectData Default { get; set; } = new();

    public Workspace DemoWorkspace { get; set; } = new();

    public List<PatentCluster> PatentClusters { get; private set; } = new();

    /// <summary>
    /// Ensures we have patent cluster data
    /// </summary>
    /// <returns></returns>
    public async Task EnsurePatentClustersLoaded()
    {
        if (this.PatentClusters.Count > 0)
        {
            return;
        }

        var jt = await PratBackend.Default.GetJsonFromGCS("patentclusters.json.gz", true);
        if (jt is JArray ja)
        {
            this.PatentClusters = ja.ToObject<List<PatentCluster>>();
        }
    }

    public async Task LoadDemoWorkspace()
    {
        this.DemoWorkspace = await PratBackend.Default.LoadWorkspace("00000000-1111-1111-1111-000000000000");
    }
}
