using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace PTTGC.Prat.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        var sentryDSN = builder.Configuration.GetValue<string>("SentryDSN");
        SentrySdk.Init(options =>
        {
            options.Dsn = sentryDSN;
#if DEBUG
            options.Debug = true;
#endif

            // This option is recommended. It enables Sentry's "Release Health" feature.
            options.AutoSessionTracking = true;

            // Enabling this option is recommended for client applications only. It ensures all threads use the same global scope.
            options.IsGlobalModeEnabled = true;

            // This option will enable Sentry's tracing features. You still need to start transactions and spans.
            options.EnableTracing = true;

            // Example sample rate for your transactions: captures 10% of transactions
            options.TracesSampleRate = 0.1;
        });

        var platformUrl = builder.Configuration.GetValue<string>("PlatformUrl");
#if DEBUG
        platformUrl = "http://localhost:7253/";
#endif
        PratBackend.Default.BaseAddress = platformUrl;

        await builder.Build().RunAsync();
    }
}
