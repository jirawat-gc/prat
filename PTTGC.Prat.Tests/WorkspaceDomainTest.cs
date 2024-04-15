using PTTGC.Prat.Backend.Domains;
using PTTGC.Prat.Core;

namespace PTTGC.Prat.Tests;

[TestClass]
public class WorkspaceDomainTest
{
    [TestInitialize]
    public void Setup()
    {
        Util.InitConfiguration();
    }

    private const string DEFAULT_USER = "00000000-1001-0000-0000-000000000000";

    [TestMethod]
    public async Task List()
    {
        var result = await WorkspaceDomain.ListWorkspaces(DEFAULT_USER);

        Assert.IsNotNull(result);

        Assert.IsTrue(result.Any(item => item.title == "test innovation"));
        Assert.IsTrue(result.Any(item => item.description == "test description"));
    }

    [TestMethod]
    public async Task Submit()
    {
        var result = await WorkspaceDomain.SaveWorkspace(new Workspace() {
            InnovationTitle = "test innovation",
            InnovationDescription = "test description"
        });
    }

    [TestMethod]
    public async Task Load()
    {
        var result = await WorkspaceDomain.SaveWorkspace(new Workspace()
        {
            InnovationTitle = "test innovation",
            InnovationDescription = "test description"
        });

        var loaded = await WorkspaceDomain.LoadWorkspace(result.OwnerId, result.Id.ToString());

        Assert.IsTrue(loaded.InnovationTitle == result.InnovationTitle);
        Assert.IsTrue(loaded.InnovationDescription == result.InnovationDescription);
        Assert.IsTrue(((double[])loaded.AIEmbeddingVector).Length == 768);
    }
}
