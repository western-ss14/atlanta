using System.Linq;
using Content.Server.Atlanta.Supply.Components;
using Content.Server.Atlanta.Supply.Events;
using Content.Shared.Atlanta.Supply.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Atlanta.Supply.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class SupplyPointSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SupplyPointComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<SupplyPointComponent, TrySpawnSupplyPointContentEvent>(OnSpawnSupply);
    }

    private void OnMapInit(Entity<SupplyPointComponent> ent, ref MapInitEvent args)
    {
        RaiseLocalEvent(new AttachSupplyPointEvent(ent.Comp.Category, ent.Owner));
    }

    private void OnSpawnSupply(Entity<SupplyPointComponent> ent, ref TrySpawnSupplyPointContentEvent args)
    {
        if (!_random.Prob(ent.Comp.Chance))
            return;

        var supply = _prototypeManager.Index(ent.Comp.SupplyPointProto);
        var supplyContentProto = _random.Pick(supply.Weights);

        var supplyContent = _prototypeManager.Index<SupplyContentPrototype>(supplyContentProto.Key);
        supplyContent.Content.ForEach(e => Spawn(e, _transform.GetMapCoordinates(ent.Owner)));
    }
}
