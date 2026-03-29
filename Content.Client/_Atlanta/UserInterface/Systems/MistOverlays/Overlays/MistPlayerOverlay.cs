using System.Numerics;
using Content.Shared.Atlanta.Mist;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using MistPlayerComponent = Content.Shared.Atlanta.Mist.Components.MistPlayerComponent;

namespace Content.Client.UserInterface.Systems.DamageOverlays.Overlays;

public sealed class MistPlayerOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> GradientCircleMaskShader = "GradientCircleMask";

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly ShaderInstance _mistShader;

    public readonly float MistLevel = 0.8f;

    public MistPlayerOverlay()
    {
        IoCManager.InjectDependencies(this);

        _mistShader = _prototypeManager.Index(GradientCircleMaskShader).InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalEntity, out EyeComponent? eyeComp))
            return;

        if (args.Viewport.Eye != eyeComp.Eye)
            return;

        var level = 0.15f;
        var minLevel = 0.15f;
        if (_entityManager.TryGetComponent<MistPlayerComponent>(_playerManager.LocalEntity, out var mist))
        {
            level = mist.MistLevel;
            minLevel = mist.MinMistLevel;
        }

        var viewport = args.WorldAABB;
        var handle = args.WorldHandle;
        var distance = args.ViewportBounds.Width;

        var time = (float)_timing.RealTime.TotalSeconds;
        var lastFrameTime = (float)_timing.FrameTime.TotalSeconds;

        var pulseRate = 3f;
        var adjustedTime = time * pulseRate;
        float outerMaxLevel = 2.0f * distance;
        float outerMinLevel = 0.8f * distance;
        float innerMaxLevel = 0.6f * distance;
        float innerMinLevel = 0.2f * distance;

        var radius = Math.Min(1.5f, MistLevel + (1f * (level / minLevel - 1)));

        var outerRadius = outerMaxLevel - radius * (outerMaxLevel - outerMinLevel);
        var innerRadius = innerMaxLevel - radius * (innerMaxLevel - innerMinLevel);

        var pulse = MathF.Max(0f, MathF.Sin(adjustedTime));

        _mistShader.SetParameter("time", pulse);
        _mistShader.SetParameter("color", new Vector3(1f, 1f, 1f));
        _mistShader.SetParameter("darknessAlphaOuter", level);

        _mistShader.SetParameter("outerCircleRadius", outerRadius);
        _mistShader.SetParameter("outerCircleMaxRadius", outerRadius + 0.2f * distance);
        _mistShader.SetParameter("innerCircleRadius", innerRadius);
        _mistShader.SetParameter("innerCircleMaxRadius", innerRadius + 0.02f * distance);
        handle.UseShader(_mistShader);
        handle.DrawRect(viewport, Color.White);
    }
}
