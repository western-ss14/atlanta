using System.Numerics;
using Content.Shared.Atlanta.Mist.Components;

namespace Content.Shared.Atlanta.Mist.Systems;

public abstract class SharedMistPlayerSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MistPlayerComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<MistPlayerComponent> ent, ref ComponentStartup args)
    {
        var queue = EntityQueryEnumerator<MistCenterPointComponent>();
        while (queue.MoveNext(out var uid, out var center))
        {
            ent.Comp.Center = _transform.GetMapCoordinates(uid);
            ent.Comp.ToleranceDistance = center.Distance;
            ent.Comp.DeathDistance = center.DeathDistance;
        }
    }

    public override void Update(float frameTime)
    {
        var queue = EntityQueryEnumerator<MistPlayerComponent>();
        while (queue.MoveNext(out var uid, out var playerMist))
        {
            var centerCoords = playerMist.Center;
            var playerCoords = _transform.GetMapCoordinates(uid);

            if (centerCoords == null)
                continue;

            var distance = Vector2.Distance(centerCoords!.Value.Position, playerCoords.Position);

            ProcessDistance(uid, playerMist, frameTime, distance);
        }
    }

    protected virtual void ProcessDistance(EntityUid target, MistPlayerComponent playerMist, float frameTime, float distance)
    {
        if (distance < playerMist.ToleranceDistance)
        {
            playerMist.MistLevel = playerMist.MinMistLevel;
        }
        else
        {
            playerMist.MistLevel = MathF.Min((int) (distance / playerMist.ToleranceDistance) * frameTime +
                                             playerMist.MistLevel,
                1f);
        }
    }
}
