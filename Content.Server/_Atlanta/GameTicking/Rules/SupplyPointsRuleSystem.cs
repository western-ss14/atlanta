using Content.Server.Atlanta.GameTicking.Rules.Components;
using Content.Server.Atlanta.Supply.Events;
using Content.Server.GameTicking.Rules;

namespace Content.Server.Atlanta.GameTicking.Rules;

/// <summary>
/// Generate supply on points
/// </summary>
public sealed class SupplyPointsRuleSystem :  GameRuleSystem<SupplyPointsRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AttachSupplyPointEvent>(OnAttachSupplyPoint);
        SubscribeLocalEvent<SpawnSupplyEvent>(OnSpawnSupply);
    }

    private void OnAttachSupplyPoint(AttachSupplyPointEvent ev)
    {
        var queue = EntityQueryEnumerator<SupplyPointsRuleComponent>();
        while (queue.MoveNext(out var rule))
        {
            if (!rule.SupplyPoints.ContainsKey(ev.Category))
            {
                rule.SupplyPoints[ev.Category] = [];
            }

            rule.SupplyPoints[ev.Category].Add(ev.Target);
        }
    }

    private void OnSpawnSupply(SpawnSupplyEvent ev)
    {
        var queue = EntityQueryEnumerator<SupplyPointsRuleComponent>();
        while (queue.MoveNext(out var rule))
        {
            var spawnEvent = new TrySpawnSupplyPointContentEvent();

            if (rule.SupplyPoints.TryGetValue(ev.Category, out var list))
            {
                list.ForEach(e => RaiseLocalEvent(e, spawnEvent));
            }
        }
    }
}
