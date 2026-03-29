using Robust.Shared.Prototypes;

namespace Content.Server.Atlanta.Player.Events;

/// <summary>
/// Try to spawn players.
/// </summary>
public sealed class SpawnQueuePlayersEvent(EntProtoId playerProto, EntProtoId spawnPointProto)
{
    public readonly EntProtoId PlayerProtoId = playerProto;
    public readonly EntProtoId SpawnPointProtoId = spawnPointProto;
}

/// <summary>
/// Enables spawning. It means, if someone entry queue, them will be spawned immediately.
/// If queue already has entries, them wil be spawned.
/// </summary>
public sealed class EnableQueueSpawningEvent(EntProtoId mobProtoId, EntProtoId spawnerProtoId)
{
    public readonly EntProtoId MobProtoId = mobProtoId;
    public readonly EntProtoId SpawnerProtoId = spawnerProtoId;
}

public sealed class DisableQueueSpawningEvent;
