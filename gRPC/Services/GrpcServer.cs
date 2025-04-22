using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;


namespace ScriptsOfTributeGRPC;

public class GrpcServer : IDisposable
{
    private WebApplication _app;
    private readonly Task _serverTask;

    public GrpcServer(string host, int port, EngineServiceAdapter engineService)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureEndpointDefaults(endpointOptions =>
            {
                endpointOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.None;
            });

            options.ListenLocalhost(port, listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
            });
        });

        builder.Services.AddGrpc();
        builder.Services.AddSingleton(engineService);

        _app = builder.Build();

        _app.MapGrpcService<EngineServiceAdapter>();

        _serverTask = _app.RunAsync();
        //Console.WriteLine($"gRPC server listening on http://{host}:{port}");
    }

    public void Dispose()
    {
        _app?.StopAsync().Wait();
        _serverTask.Wait();
    }
}
