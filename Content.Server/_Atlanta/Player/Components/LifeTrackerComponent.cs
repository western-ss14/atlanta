namespace Content.Server.Atlanta.Player;

/// <summary>
/// This is used for count life time.
/// </summary>
[RegisterComponent]
public sealed partial class LifeTrackerComponent : Component
{
    [DataField]
    public bool IsDead = false;

    [DataField]
    public TimeSpan StartupTime;

    [DataField]
    public TimeSpan DeathTime;

    public TimeSpan Lifetime => DeathTime - StartupTime;
}
