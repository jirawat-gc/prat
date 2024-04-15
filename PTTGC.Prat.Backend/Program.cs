using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            code = errorDetail?.code ?? 2000,
            message = errorDetail?.message,
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
        builder.Services.AddCors();

        builder.Configuration.Bind(Settings.Instance);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );
        }
        else
        {
            app.UseCors(builder => builder
                .WithOrigins(
                    "https://prat.askmex.com",
                    "http://localhost:5198")
                .AllowAnyHeader()
                .AllowAnyHeader()
            );
        }

        // we like Newtonsoft JSON more than System.Text.Json
        Func<HttpContext, Task<JObject>> readBodyAsJSON = async (ctx) =>
        {
            using var sr = new StreamReader(ctx.Request.Body);
            using var jr = new JsonTextReader(sr);

            var jo = await JObject.ReadFromAsync(jr);
            return jo as JObject;
        };

        app.MapPost("/workspace", async (HttpContext ctx) =>
        {
            return await HandleRequest(ctx, async () =>
            {
                var jo = await readBodyAsJSON(ctx);
                var ws = jo.ToObject<Workspace>()!;

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
                var clusters = await PatentClusterDomain.GetPatentClusterSignedUrls();
                return clusters;
            });
        })
        .WithName("Get Signed URLs to load patent clusters")
        .WithOpenApi();

        app.MapPost("/prompt", async (HttpContext ctx) =>
        {
            return await HandleRequest(ctx, async () =>
            {
                var jo = await readBodyAsJSON(ctx);
                var p = jo.ToObject<PromptRequest>()!;

                var response = await VertexAIDomain.GetCompletion(p);
                return response;
            });
        })
        .WithName("Perform Text Generation with Prompt")
        .WithOpenApi();

        app.MapPost("/embeddings", async (HttpContext ctx) =>
        {
            return await HandleRequest(ctx, async () =>
            {
                var jo = await readBodyAsJSON(ctx);
                var emb = jo.ToObject<EmbeddingRequest>()!;

                var response = await VertexAIDomain.GetEmbeddings(emb.Content);
                return response;
            });
        })
        .WithName("Create Embeddings from given content")
        .WithOpenApi();

        app.Run();
    }
}
