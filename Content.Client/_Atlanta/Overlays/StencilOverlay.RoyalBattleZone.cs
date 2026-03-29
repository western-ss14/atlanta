using System.Numerics;
using Content.Shared.Atlanta.RoyalBattle.Components;
using Robust.Client.Graphics;
using Robust.Client.Rendering;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Client.GameObjects;

namespace Content.Client.Overlays;

public sealed partial class StencilOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ParallaxSystem _parallax = default!;

    private static readonly ProtoId<ShaderPrototype> StencilMaskShader = "StencilMask";
    private static readonly ProtoId<ShaderPrototype> StencilDrawShader = "StencilDraw";

    private ShaderInstance? _shader;
    private CachedResources? _resources;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public StencilOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_shader == null)
        {
            var shaderProto = _protoManager.Index(new ProtoId<ShaderPrototype>("StencilZone"));
            _shader = shaderProto.Instance();
        }

        if (_resources == null)
        {
            _resources = new CachedResources(args.WorldHandle);
        }

        var query = _entityManager.AllEntityQueryEnumerator<RbZoneComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var rbZone, out var transform))
        {
            if (!rbZone.IsEnabled)
                continue;

            var invMatrix = Matrix3x2.CreateTranslation(-transform.WorldPosition);
            DrawRoyalBattleZone(in args, _resources, rbZone, invMatrix);
        }
    }

    private void DrawRoyalBattleZone(in OverlayDrawArgs args, CachedResources res, RbZoneComponent rbZoneComponent,
        Matrix3x2 invMatrix)
    {
        var worldHandle = args.WorldHandle;
        var renderScale = args.Viewport.RenderScale.X;
        var zoom = args.Viewport.Eye?.Zoom ?? Vector2.One;
        var length = zoom.X;
        var bufferRange = MathF.Min(10f, rbZoneComponent.RangeLerp);

        var pixelCenter = Vector2.Transform(rbZoneComponent.Center, invMatrix);
        var vertical = args.Viewport.Size.Y;

        var pixelMaxRange = rbZoneComponent.RangeLerp * renderScale / length * EyeManager.PixelsPerMeter;
        var pixelBufferRange = bufferRange * renderScale / length * EyeManager.PixelsPerMeter;
        var pixelMinRange = pixelMaxRange - pixelBufferRange;

        _shader!.SetParameter("position", new Vector2(pixelCenter.X, vertical - pixelCenter.Y));
        _shader.SetParameter("maxRange", pixelMaxRange);
        _shader.SetParameter("minRange", pixelMinRange);
        _shader.SetParameter("bufferRange", pixelBufferRange);
        _shader.SetParameter("gradient", 0.8f);

        var worldAABB = args.WorldAABB;
        var worldBounds = args.WorldBounds;
        var position = args.Viewport.Eye?.Position.Position ?? Vector2.Zero;
        var localAABB = invMatrix.TransformBox(worldAABB);

        worldHandle.RenderInRenderTarget(res.Blep!, () =>
        {
            worldHandle.UseShader(_shader);
            worldHandle.DrawRect(localAABB, Color.White);
        }, Color.Transparent);

        worldHandle.SetTransform(Matrix3x2.Identity);

        var maskShader = _protoManager.Index(StencilMaskShader).Instance();
        worldHandle.UseShader(maskShader);
        worldHandle.DrawTextureRect(res.Blep!.Texture, worldBounds);

        var curTime = _timing.RealTime;
        var sprite = _sprite.GetFrame(rbZoneComponent.ZoneTexture, curTime);

        var drawShader = _protoManager.Index(StencilDrawShader).Instance();
        worldHandle.UseShader(drawShader);
        _parallax.DrawParallax(worldHandle, worldAABB, sprite, curTime, position, new Vector2(0.5f, 0f), modulate: Color.White);
    }

    private class CachedResources
    {
        public IRenderTexture Blep { get; }

        public CachedResources(DrawingHandleScreen handle)
        {
            Blep = handle.CreateRenderTarget(new Vector2i(512, 512), RenderTargetFormatParameters.Default);
        }
    }
}
