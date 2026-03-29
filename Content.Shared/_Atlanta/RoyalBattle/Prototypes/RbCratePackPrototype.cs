using Robust.Shared.Prototypes;

namespace Content.Shared.Atlanta.RoyalBattle.Prototypes;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype]
public sealed partial class RbCratePackPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("pack", required: true)]
    public List<EntProtoId> Pack = new();
}
