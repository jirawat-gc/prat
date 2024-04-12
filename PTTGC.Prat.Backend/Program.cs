
namespace PTTGC.Prat.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        //builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapPost("/workspace", (HttpContext httpContext) =>
        {
            // submit workspace for processing

            return 200;
        })
        .WithName("Submit Workspace")
        .WithOpenApi();

        app.MapGet("/clusters", (HttpContext httpContext) =>
        {
            // Get clusters 
            // submit workspace for processing

            return 200;
        })
        .WithName("Get Clusters")
        .WithOpenApi();

        app.MapGet("/clusters/{cluster_id}", (HttpContext httpContext) =>
        {
            // Get clusters members
            return 200;
        })
        .WithName("Get Cluster Member")
        .WithOpenApi();


        app.MapGet("/clusters/{cluster_id}", (HttpContext httpContext) =>
        {
            // Get clusters members
            return 200;
        })
        .WithName("Get Cluster Member")
        .WithOpenApi();

        app.MapGet("/clusters/{cluster_id}", (HttpContext httpContext) =>
        {
            // Get clusters members
            return 200;
        })
        .WithName("Get Cluster Member")
        .WithOpenApi();


        app.MapPost("/analysis", (HttpContext httpContext) =>
        {
            // Get analysis of the patent comparing to another patent
            return 200;
        })
        .WithName("Get Analysis")
        .WithOpenApi();

        app.Run();
    }
}
