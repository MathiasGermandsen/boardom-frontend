namespace Boardom.Services;

public sealed class PendingDeviceStore
{
    private readonly object _lock = new();
    private string? _pendingDeviceId;
    private DateTimeOffset? _connectedAt;

    public string? PendingDeviceId
    {
        get { lock (_lock) return _pendingDeviceId; }
    }

    public DateTimeOffset? ConnectedAt
    {
        get { lock (_lock) return _connectedAt; }
    }

    public event Action? Changed;

    public void SetConnected(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return;

        lock (_lock)
        {
            _pendingDeviceId = deviceId.Trim();
            _connectedAt = DateTimeOffset.UtcNow;
        }

        Changed?.Invoke();
    }
    public void Clear()
    {
        lock (_lock)
        {
            _pendingDeviceId = null;
            _connectedAt = null;
        }
        
        Changed?.Invoke();
    }
}