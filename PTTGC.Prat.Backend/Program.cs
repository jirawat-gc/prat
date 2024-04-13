using PTTGC.Prat.Backend.Domains;
using PTTGC.Prat.Common;
using PTTGC.Prat.Common.Requests;
using PTTGC.Prat.Common.Response;
using PTTGC.Prat.Core;

namespace PTTGC.Prat.Backend;

public class Program
{
    private static async Task<GenericResponse<TResult>> HandleRequest<TResult>(HttpContext context, Func<Task<TResult>> handler)
    {
        TResult? result = default;
        ErrorDetail? errorDetail = null;

        try
        {
            result = await handler();
        }
        catch (ExceptionWithErrorDetail ex)
        {
            errorDetail = ex.Detail;
            context.Response.StatusCode = ex.Detail.code;
        }

        return new GenericResponse<TResult>()
        {
            data = result,
            code = errorDetail.code,
            message = errorDetail.message,
        };
    }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        //builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Configuration.Bind(Settings.Instance);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapPost("/workspace", async (Workspace ws, HttpContext ctx) =>
        {
            return await HandleRequest(ctx, async () =>
            {
                // submit workspace for processing
                var processedWorkspace = await WorkspaceDomain.SubmitWorkspace(ws);
                return processedWorkspace;
            });
        })
        .WithName("Submit Workspace")
        .WithOpenApi();

        app.MapGet("/clusters", async (HttpContext ctx) =>
        {
            return await HandleRequest(ctx, async () =>
            {
                var clusters = await PatentClusterDomain.GetClusters();
                return clusters;
            });
        })
        .WithName("Get Patent Clusters for Rendering")
        .WithOpenApi();

        app.MapPost("/findcluster", async (FindClusterRequest fcr, HttpContext ctx) =>
        {
            return await HandleRequest(ctx, async () =>
            {
                // submit workspace for processing
                var localCluster = await PatentClusterDomain.GetLocalCluster(fcr.SummaryEmbeddingVector, fcr.Flags);
                return localCluster;
            });
        })
        .WithName("Find Local Cluster")
        .WithOpenApi();

        app.MapGet("/clusters/{clusterLabel}/members", async (string clusterLabel, HttpContext ctx) =>
        {
            return await HandleRequest(ctx, async () =>
            {
                var patents = await PatentClusterDomain.GetClusterMember(clusterLabel);
                return patents;
            });
        })
        .WithName("Get Cluster Member")
        .WithOpenApi();

        app.MapPost("/prompt", async (PromptRequest p, HttpContext ctx) =>
        {
            return await HandleRequest(ctx, async () =>
            {
                var response = await VertexAIDomain.GetCompletion(p);
                return response;
            });
        })
        .WithName("Perform Text Generation with Prompt")
        .WithOpenApi();

        app.MapPost("/embeddings", async (EmbeddingRequest emb, HttpContext ctx) =>
        {
            return await HandleRequest(ctx, async () =>
            {
                var response = await VertexAIDomain.GetEmbeddings(emb.Content);
                return response;
            });
        })
        .WithName("Create Embeddings from given content")
        .WithOpenApi();

        app.Run();
    }
}
