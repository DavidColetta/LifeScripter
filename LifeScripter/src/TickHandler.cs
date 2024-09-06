class TickHandler {
    public delegate void TickDelegate();
    public event TickDelegate? OnTick;

    public void FireTick() {
        OnTick?.Invoke();
    }

    public static TickHandler operator +(TickHandler kElement, TickDelegate kDelegate)
    {
        kElement.OnTick += kDelegate;
        return kElement;
    }

    public static TickHandler operator -(TickHandler kElement, TickDelegate kDelegate)
    {
        kElement.OnTick -= kDelegate;
        return kElement;
    }
}