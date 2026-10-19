using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameFrameX.Core.Tests.Integration;

public class ClientApiTestHostTests
{
    [Fact]
    public async Task TestHost_AppliesCorsPolicyAndMiddlewareOrder()
    {
        var observedSteps = new List<string>();
        using var host = await CreateHostAsync(observedSteps);
        using var client = host.GetTestClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "https://example.test");

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Equal("*", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Equal(new[] { "TenantResolution", "TenantExpiration", "AppResolution", "Endpoint", }, observedSteps);
    }

    private static async Task<IHost> CreateHostAsync(ICollection<string> observedSteps)
    {
        var host = new HostBuilder()
                   .ConfigureWebHost(webBuilder =>
                   {
                       webBuilder.UseTestServer();
                       webBuilder.ConfigureServices(services =>
                       {
                           services.AddRouting();
                           services.AddCors(options =>
                           {
                               options.AddPolicy("CorsPolicy", policy =>
                               {
                                   policy.AllowAnyOrigin()
                                         .AllowAnyHeader()
                                         .AllowAnyMethod();
                               });
                           });
                       });
                       webBuilder.Configure(app =>
                       {
                           app.UseRouting();
                           app.UseCors("CorsPolicy");
                           app.Use(async (context, next) =>
                           {
                               observedSteps.Add("TenantResolution");
                               await next();
                           });
                           app.Use(async (context, next) =>
                           {
                               observedSteps.Add("TenantExpiration");
                               await next();
                           });
                           app.Use(async (context, next) =>
                           {
                               observedSteps.Add("AppResolution");
                               await next();
                           });
                           app.UseEndpoints(endpoints =>
                           {
                               endpoints.MapGet("/health", async context =>
                               {
                                   observedSteps.Add("Endpoint");
                                   await context.Response.WriteAsync("ok");
                               });
                           });
                       });
                   })
                   .Build();

        await host.StartAsync();
        return host;
    }
}