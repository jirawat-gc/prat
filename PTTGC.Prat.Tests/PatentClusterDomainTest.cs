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
}
