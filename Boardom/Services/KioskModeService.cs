namespace Boardom.Services;

public class KioskModeService
{
    public bool IsKioskMode { get; }

    public KioskModeService()
    {
        var value = Environment.GetEnvironmentVariable("KIOSK_MODE");
        IsKioskMode = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
