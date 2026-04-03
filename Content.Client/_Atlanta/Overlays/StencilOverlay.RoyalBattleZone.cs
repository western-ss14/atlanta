using System.Numerics;
using Content.Shared.Atlanta.RoyalBattle.Components;
using Robust.Client.Graphics;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;

namespace Content.Client.Overlays;

public sealed partial class StencilOverlay
{
    private static readonly ProtoId<ShaderPrototype> StencilMaskShader = "StencilMask";
    private static readonly ProtoId<ShaderPrototype> StencilDrawShader = "StencilDraw";
    private void DrawRoyalBattleZone(in OverlayDrawArgs args, CachedResources res, RbZoneComponent rbZoneComponent,
        Matrix3x2 invMatrix)
    {
        var worldHandle = args.WorldHandle;
        var renderScale = args.Viewport.RenderScale.X;
        // TODO: This won't handle non-standard zooms so uhh yeah, not sure how to structure it on the shader side.
        var zoom = args.Viewport.Eye?.Zoom ?? Vector2.One;
        var length = zoom.X;
        var bufferRange = MathF.Min(10f, rbZoneComponent.RangeLerp);

        var pixelCenter = Vector2.Transform(rbZoneComponent.Center, invMatrix);
        // Something something offset?
        var vertical = args.Viewport.Size.Y;

        var pixelMaxRange = rbZoneComponent.RangeLerp * renderScale / length * EyeManager.PixelsPerMeter;
        var pixelBufferRange = bufferRange * renderScale / length * EyeManager.PixelsPerMeter;
        var pixelMinRange = pixelMaxRange - pixelBufferRange;

        _shader.SetParameter("position", new Vector2(pixelCenter.X, vertical - pixelCenter.Y));
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
        worldHandle.UseShader(_protoManager.Index(StencilMaskShader).Instance());
        worldHandle.DrawTextureRect(res.Blep!.Texture, worldBounds);
        var curTime = _timing.RealTime;
        var sprite = _sprite.GetFrame(rbZoneComponent.ZoneTexture, curTime);

        // Draw the rain
        worldHandle.UseShader(_protoManager.Index(StencilDrawShader).Instance());
        _parallax.DrawParallax(worldHandle, worldAABB, sprite, curTime, position, new Vector2(0.5f, 0f), modulate: Color.White);
    }
}
