using Content.Server.KillTracking;
using Robust.Shared.Timing;

namespace Content.Server.Atlanta.Player.Systems;

/// <summary>
/// This is used to record entity lifetime.
/// </summary>
public sealed class LifeTimeTrackerSystem : EntitySystem
{
    /// <inheritdoc/>

    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LifeTrackerComponent, ComponentStartup>(OnComponentStartup);

        SubscribeLocalEvent<KillReportedEvent>(OnKillReported);
    }

    private void OnKillReported(ref KillReportedEvent ev)
    {
        if (!TryComp<LifeTrackerComponent>(ev.Entity, out var lifetime))
            return;

        lifetime.IsDead = true;
        lifetime.DeathTime = _gameTiming.CurTime;
    }

    private void OnComponentStartup(Entity<LifeTrackerComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.StartupTime = _gameTiming.CurTime;
        ent.Comp.DeathTime = ent.Comp.StartupTime;
    }
}
