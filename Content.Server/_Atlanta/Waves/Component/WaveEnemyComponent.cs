namespace Content.Server.Atlanta.Waves.Component;

/// <summary>
/// This is used for tracking wave enemies.
/// </summary>
[RegisterComponent]
public sealed partial class WaveEnemyComponent : Robust.Shared.GameObjects.Component
{
    [DataField("queuedToDelete")]
    public bool IsQueuedToDelete = false;

    [DataField]
    public TimeSpan DeletingTime = TimeSpan.FromSeconds(5);
}
