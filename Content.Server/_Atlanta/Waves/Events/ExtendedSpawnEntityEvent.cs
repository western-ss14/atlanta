using Robust.Shared.Prototypes;

namespace Content.Server.Atlanta.Waves.Events;

[ByRefEvent]
public sealed class ExtendedSpawnEntityEvent(EntProtoId entityProtoId, EntProtoId spawner)
{
    public readonly EntProtoId EntityProtoId = entityProtoId;
    public readonly EntProtoId SpawnerProtoId = spawner;

    public EntityUid? Instance = null;
}
