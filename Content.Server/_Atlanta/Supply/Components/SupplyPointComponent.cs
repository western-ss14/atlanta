using Content.Shared.Atlanta.Supply.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Atlanta.Supply.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class SupplyPointComponent : Component
{
    [DataField(required: true)]
    public string Category = default!;

    [DataField("supplyProto", required: true)]
    public ProtoId<SupplyPointPrototype> SupplyPointProto = default!;

    [DataField]
    public float Chance = 0.4f;
}
