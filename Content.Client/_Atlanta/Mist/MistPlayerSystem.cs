using Content.Client.UserInterface.Systems.DamageOverlays.Overlays;
using Content.Shared.Atlanta.Mist;
using Content.Shared.Atlanta.Mist.Systems;
using Content.Shared.Mobs.Components;
using Robust.Client.Graphics;
using Robust.Shared.Player;

namespace Content.Client.Atlanta.Mist;

/// <summary>
/// This handles...
/// </summary>
public sealed class MistPlayerSystem : SharedMistPlayerSystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    private MistPlayerOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new MistPlayerOverlay();

        SubscribeLocalEvent<Shared.Atlanta.Mist.Components.MistPlayerComponent, LocalPlayerAttachedEvent>(OnPlayerAttach);
        SubscribeLocalEvent<Shared.Atlanta.Mist.Components.MistPlayerComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnPlayerAttach(Entity<Shared.Atlanta.Mist.Components.MistPlayerComponent> _, ref LocalPlayerAttachedEvent ev)
    {
        if (!HasComp<MobStateComponent>(ev.Entity))
            return;
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<Shared.Atlanta.Mist.Components.MistPlayerComponent> _, ref LocalPlayerDetachedEvent ev)
    {
        _overlayManager.RemoveOverlay(_overlay);
    }
}
