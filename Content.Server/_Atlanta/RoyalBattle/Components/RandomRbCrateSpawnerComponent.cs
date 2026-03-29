using Robust.Shared.Prototypes;

namespace Content.Server.Atlanta.RoyalBattle.Components;

[RegisterComponent]
public sealed partial class RandomRbCrateSpawnerComponent : Component
{
    [DataField("proto")]
    public EntProtoId PrototypeId = "RandomCrateRoyalBattle";

    [DataField("chance")]
    public float Chance = 0.5f;
}
