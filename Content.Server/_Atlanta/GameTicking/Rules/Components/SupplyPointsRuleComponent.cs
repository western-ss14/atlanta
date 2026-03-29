using Content.Server.Atlanta.Supply.Components;

namespace Content.Server.Atlanta.GameTicking.Rules.Components;

/// <summary>
/// Generate supply on points
/// </summary>
[RegisterComponent, Access(typeof(SupplyPointsRuleSystem))]
public sealed partial class SupplyPointsRuleComponent : Component
{
    /// <summary>
    /// Contains attached supply points with category name.
    /// </summary>
    public readonly Dictionary<string, List<EntityUid>> SupplyPoints = [];
}
