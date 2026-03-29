using Robust.Shared.Prototypes;

namespace Content.Shared.Atlanta.Player.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class ExtendedSpawnActionsComponent : Component
{
    public EntProtoId JoinProtoId;
    public EntProtoId LeaveProtoId;

    [DataField]
    public EntityUid? ActionId;
}
