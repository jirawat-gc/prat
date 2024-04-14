using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Backend.Domains;
using PTTGC.Prat.Common.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTTGC.Prat.Tests;

[TestClass]
public class PatentClusterDomainTest
{
    [TestInitialize]
    public void Setup()
    {
        Util.InitConfiguration();
    }

    [TestMethod]
    public async Task GetSignedUrls()
    {
        var result = await PatentClusterDomain.GetPatentClusterSignedUrls();

        Assert.IsTrue(result.Urls != null);
        Assert.IsTrue(result.Urls.Count > 0);
        Assert.IsTrue(result.Expiry > DateTimeOffset.Now);
        Assert.IsTrue(result.Urls.All(u => u.Contains("patentcluster")));

        var secondCached = await PatentClusterDomain.GetPatentClusterSignedUrls();

        Assert.IsTrue(result.Expiry == secondCached.Expiry);
    }

    [TestMethod]
    public async Task TestFeatureFlagBuilder()
    {
        var result = FindClusterRequest.BuildFeatureFlagArray(new Dictionary<string, bool>()
        {
            {  "chemical", true },
            {  "polymer", true },
        }).ToList();

        Assert.IsTrue(result[0] == -1.5);
        Assert.IsTrue(result[1] == 1.5);
    }

    [TestMethod]
    public async Task TestFindClusterFromFeature()
    {
        // Cluster 44
        // Patent ID 303000201
        var summary = "The patent describes a rice hulling machine that includes a frame, a hopper for holding rice husks, a set of rubber rollers for hulling the rice, a fan for separating the hulls from the rice, and a chamber for collecting the hulls.\r\n\r\nThe machine also includes a feed plate for distributing the rice and hulls to the fan, and a series of baffles in the chamber for separating the hulls from the rice.\r\n\r\nThe fan is connected to the feed plate and is driven by an external power source.";
        var embedding = await VertexAIDomain.GetEmbeddings(summary);

        var request = new FindClusterRequest(embedding, new Dictionary<string, bool>()
        {
            {  "argriculture", true },
            {  "has_manufacturing_process", true },
        });

        Assert.IsTrue( request.FeatureFlags.Length == 20);
        Assert.IsTrue(request.FeatureFlags[4] == 1);
        Assert.IsTrue(request.FeatureFlags[19] == 1);
        Assert.IsTrue(request.EmbeddingVector.Length == 768);

        var json = JsonConvert.SerializeObject( request, Formatting.None );
    }
}
