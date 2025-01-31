using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


namespace ScriptsOfTributeGRPC;

public class GrpcServer : IDisposable
{
    private WebApplication _app;

    public GrpcServer(string host, int port, EngineServiceAdapter engineService)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddGrpc();

        _app = builder.Build();

        _app.MapGrpcService<EngineServiceAdapter>();

        _app.RunAsync($"http://{host}:{port}");
        Console.WriteLine($"gRPC server listening on http://{host}:{port}");
    }

    public void Dispose()
    {
        _app?.StopAsync().Wait();
    }
}
