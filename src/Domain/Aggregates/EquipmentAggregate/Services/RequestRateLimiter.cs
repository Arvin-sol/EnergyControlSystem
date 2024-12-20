using Domain.Aggregates.EquipmentAggregate.Contracts;

namespace Domain.Aggregates.EquipmentAggregate.Services;

public class RequestRateLimiter : IRequestRateLimiter
{
    private const int RequestLimit = 100;
    private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(1);
    private readonly Dictionary<string, Queue<DateTime>> _requestLogs = new();

    public bool CanProcessRequest(string clientId)
    {
        if (!_requestLogs.ContainsKey(clientId))
            _requestLogs[clientId] = new Queue<DateTime>();

        var requests = _requestLogs[clientId];
        var now = DateTime.UtcNow;

        while (requests.Count > 0 && (now - requests.Peek()) > TimeWindow)
            requests.Dequeue();


        if (requests.Count >= RequestLimit)
            return false;

        requests.Enqueue(now);
        return true;
    }
}
