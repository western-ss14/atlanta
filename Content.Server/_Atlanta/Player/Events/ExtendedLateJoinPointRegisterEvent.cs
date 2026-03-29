using Content.Server.Atlanta.GameTicking.Rules;
using Robust.Shared.Prototypes;

namespace Content.Server.Atlanta.Player.Events;

/// <summary>
/// Attach spawn to <see cref="ExtendedLateJoinRuleSystem"/>.
/// </summary>
public sealed class ExtendedLateJoinPointRegisterEvent(EntityUid spawner, EntProtoId protoId)
{
    public readonly EntityUid SpawnerEnt = spawner;
    public readonly EntProtoId ProtoId = protoId;
}
