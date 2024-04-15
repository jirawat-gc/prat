using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTTGC.Prat.Backend;
using PTTGC.Prat.Backend.Domains;
using PTTGC.Prat.Common;
using PTTGC.Prat.Common.Requests;

namespace PTTGC.Prat.Tests;

[TestClass]
public class VertexAIDomainTest
{
    [TestInitialize]
    public void Setup()
    {
        Util.InitConfiguration();
    }

    [TestMethod]
    public async Task Completion()
    {
        var region = new string[]
        {
            "us-central1",
            "us-west4",
            "us-east4",
            "us-west1",
            "asia-northeast3",
            //"asia-southeast1",
            "asia-northeast1",
        };

        foreach (var item in region)
        {
            var result = await VertexAIDomain.GetCompletion(new PromptRequest()
            {
                PromptKey = "DEBUG",
                PromptContext = JObject.FromObject(new
                {
                    prompt_text = "this is a test, respond with OK"
                }),
                MaxTokens = 10,
                Region = item,
            });

            Assert.IsTrue(result.ToLowerInvariant().Contains("ok"));
        }
    }

    [TestMethod]
    public async Task Embeddings()
    {
        var result = await VertexAIDomain.GetEmbeddings("this is a test");

        Assert.IsTrue(result.Length >= 768);
    }

    [TestMethod]
    public async Task Embeddings_Real()
    {
        var result = await VertexAIDomain.GetEmbeddings("This patent describes a method for making freeze-dried stir-fried basil, also known as stir-fried basil semi-finished products or dried stir-fried basil. The method involves stir-frying meat or a meat substitute with seasonings, then adding stir-fried basil, chili, onion, and green beans. The mixture is then frozen and vacuum-dried until it is dry or has a low moisture content (less than 9% by weight). The dried product is then packaged in an airtight container.");

        var json = JArray.FromObject(result).ToString(Formatting.None);

        Assert.IsTrue(result.Length >= 768);
    }

    [TestMethod]
    public async Task EmbeddingsEnCode()
    {
        var result = await VertexAIDomain.GetEmbeddings("this is a test");
        Assert.IsTrue(result.Length >= 768);

        var base64 = VectorEmbedding.Encode(result);
        var decoded = VectorEmbedding.Decode(base64);

        Assert.IsTrue( result.SequenceEqual(decoded) );
    }

    [TestMethod]
    public async Task EmbeddingsJsonConvert()
    {
        var result = await VertexAIDomain.GetEmbeddings("this is a test");
        Assert.IsTrue(result.Length >= 768);

        var json = JsonConvert.SerializeObject(new
        {
            vector = (VectorEmbedding)result
        });

        var jo = JObject.Parse(json);
        var base64 = jo["vector"].ToString();
        var decoded = VectorEmbedding.Decode(base64);

        Assert.IsTrue(result.SequenceEqual(decoded));
    }

    public class TestClassWithVectorEmbedding
    {
        public VectorEmbedding Vector { get; set; }
    }

    [TestMethod]
    public async Task EmbeddingsJsonDeserialize()
    {
        var result = await VertexAIDomain.GetEmbeddings("this is a test");
        Assert.IsTrue(result.Length >= 768);

        var json = JsonConvert.SerializeObject(new TestClassWithVectorEmbedding
        {
            Vector = (VectorEmbedding)result
        });

        var instance = JsonConvert.DeserializeObject<TestClassWithVectorEmbedding>(json);   

        Assert.IsTrue(result.SequenceEqual((double[])instance.Vector));
    }
}