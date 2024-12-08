using Grpc.Core;
using Grpc.Net.Client;
using System;
using ScriptsOfTribute;


namespace ScriptsOfTributeGRPC;

public class AIServiceAdapter : IDisposable
{
    private readonly AIService.AIServiceClient _client;
    private readonly GrpcChannel _channel;
    public AIServiceAdapter(string address = "localhost:5000")
    {
        _channel = GrpcChannel.ForAddress(address);
        _client = new AIService.AIServiceClient(_channel);
    }

    public void PrepareGame()
    {
        _client.PregamePrepare(new Empty() { });
    }

    public PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        var request = new SelectPatronRequest
        {
            Round = round
        };

        request.AvailablePatrons.AddRange(availablePatrons);

        var response = _client.SelectPatron(request);
        return response.PatronId;
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
