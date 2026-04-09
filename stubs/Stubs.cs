// Minimal stub of OPERATOR game types needed to compile the plugin.
// At runtime, BepInEx resolves these against the real GameAssembly via IL2CPP interop.
// Do NOT ship this DLL — it is only used at compile time.

public class PlayerNetworking
{
    public int magcount;
}

public class MagManagerUI
{
    public void RefreshMags() { }
    public void RefillAllMags() { }
}
