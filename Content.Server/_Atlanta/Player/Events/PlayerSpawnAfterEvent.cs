namespace Content.Server.Atlanta.Player.Events;

/// <summary>
/// Raise local to ensure others system that player has been spawned.
/// </summary>
public sealed class PlayerSpawnAfterEvent(EntityUid ent)
{
    public readonly EntityUid PlayerEntity = ent;
}
