namespace Boardom.Services;

public class KioskModeService
{
    public bool IsKioskMode { get; private set; }

    public event Action? OnChange;

    public void EnableKioskMode()
    {
        IsKioskMode = true;
        OnChange?.Invoke();
    }

    public void DisableKioskMode()
    {
        IsKioskMode = false;
        OnChange?.Invoke();
    }
}
