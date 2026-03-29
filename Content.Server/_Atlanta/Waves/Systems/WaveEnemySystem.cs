using Content.Server.Atlanta.Waves.Component;
using Content.Server.Atlanta.Waves.Events;
using Content.Shared.Mobs;
using Robust.Shared.Timing;

namespace Content.Server.Atlanta.Waves.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class WaveEnemySystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<WaveEnemyComponent, MobStateChangedEvent>(OnEnemyStateChanged);
    }

    private void OnEnemyStateChanged(Entity<WaveEnemyComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.Component.CurrentState == MobState.Dead)
        {
            ent.Comp.IsQueuedToDelete = true;

            var ev = new EnemyDeadReportEvent(ent.Owner, _timing.CurTime + ent.Comp.DeletingTime);
            RaiseLocalEvent(ev);
        }
    }
}
