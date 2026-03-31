using Content.Server.Atlanta.GameTicking.Rules.Components;
using Content.Shared.Actions;
using Content.Shared.Atlanta.Player.Components;

namespace Content.Server.Atlanta.Player.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class ExtendedSpawnActionsSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ExtendedSpawnActionsComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<ExtendedSpawnActionsComponent> ent, ref ComponentStartup args)
    {
        var queue = AllEntityQuery<ExtendedLateJoinRuleComponent>();

        while (queue.MoveNext(out var rule))
        {
            ent.Comp.JoinProtoId = rule.JoinAction;
            ent.Comp.LeaveProtoId = rule.LeaveAction;
            _actions.AddAction(ent.Owner, ref ent.Comp.ActionId, rule.JoinAction);
        }
    }
}
