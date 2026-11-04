using BossMod.Autorotation;
using BossMod.Data;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal class OverlayService(
    Camera camera,
    IFramework framework,
    ConfigRoot configRoot,
    WorldState worldstate,
    AIHints hintsGlobal
) : IHostedService
{
    private readonly OverlayConfig _uiConfig = configRoot.Get<OverlayConfig>();
    private readonly AutorotationConfig _arConfig = configRoot.Get<AutorotationConfig>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        framework.Update += Draw;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        framework.Update -= Draw;
    }

    bool IsMeleeSorta(Actor player) => player.Class == Class.RDM || player.Class.GetRole() is Role.Tank or Role.Melee;

    private void Draw(IFramework framework)
    {
        if (worldstate.Party.Player() is not { } player)
            return;

        var inDuty = worldstate.CurrentCFCID > 0;
        var inCombat = player.InCombat;

        var showPixel = _uiConfig.ShowPixel switch
        {
            OverlayConfig.PixelBehavior.Combat => inCombat,
            OverlayConfig.PixelBehavior.Duty => inDuty,
            OverlayConfig.PixelBehavior.Either => inCombat || inDuty,
            OverlayConfig.PixelBehavior.Both => inCombat && inDuty,
            _ => false
        };

        if (showPixel)
            camera.AddCircle(player.PosRot.XYZ(), 0, _uiConfig.PixelColor.ABGR, _uiConfig.PixelSize, false);

        var showRing = _uiConfig.ShowTargetRing switch
        {
            OverlayConfig.RingBehavior.Melee => IsMeleeSorta(player),
            OverlayConfig.RingBehavior.Everyone => true,
            _ => false
        };

        if (showRing && worldstate.Actors.Find(player.TargetID) is { IsAlly: false } target)
            DrawTargetRing(player, target);

        if (_arConfig.ShowPositionals)
        {
            var p = hintsGlobal.RecommendedPositional;
            if (p.Target != null)
                DrawPositionals(player, p.Target, p.Pos, p.Imminent, p.Correct);
        }
    }

    void DrawTargetRing(Actor player, Actor target)
    {
        var effectiveDistance = IsMeleeSorta(player) ? 3 : 25;

        var radius = target.HitboxRadius + player.HitboxRadius + effectiveDistance;
        var rot = target.Rotation;
        var tpos = target.PosRot.XYZ();

        var showAuto = _uiConfig.ShowAutos switch
        {
            OverlayConfig.AutoRingBehavior.Tank => player.Class.GetRole() == Role.Tank,
            OverlayConfig.AutoRingBehavior.Everyone => true,
            _ => false
        };

        if (showAuto)
        {
            if (EnemyBehavior.TryGet(target.OID, out var rec))
            {
                if (rec.CanMove)
                    camera.AddCircle(tpos, target.HitboxRadius + rec.TankDistance + player.HitboxRadius, _uiConfig.FallbackColor.ABGR, 2);
            }
            else
            {
                camera.AddCircle(tpos, target.HitboxRadius + player.HitboxRadius + 2, _uiConfig.FallbackColor.ABGR, 2);
            }
        }

        if (player.Role != Role.Melee)
        {
            camera.AddCircle(tpos, radius, _uiConfig.FallbackColor.ABGR, 2);

            if (player.Class == Class.RDM && _uiConfig.ShowTargetRing == OverlayConfig.RingBehavior.Everyone)
                camera.AddCircle(tpos, target.HitboxRadius + player.HitboxRadius + 25, _uiConfig.FallbackColor.ABGR, 2);

            return;
        }

        camera.AddArc(tpos, radius, rot + 180.Degrees(), 45.Degrees(), _uiConfig.RearColor.ABGR, 2);
        camera.AddArc(tpos, radius, rot + 90.Degrees(), 45.Degrees(), _uiConfig.FlankColor.ABGR, 2);
        camera.AddArc(tpos, radius, rot - 90.Degrees(), 45.Degrees(), _uiConfig.FlankColor.ABGR, 2);
        camera.AddArc(tpos, radius, rot, 45.Degrees(), _uiConfig.FrontColor.ABGR, 2);

        if (_uiConfig.HighlightSlice)
        {
            var tt = target.Position - player.Position;
            var ta = MathF.Atan2(tt.X, tt.Z);
            var tb = target.Rotation.Rad - ta;
            while (tb > MathF.PI)
                tb -= MathF.PI * 2;
            while (tb < -MathF.PI)
                tb += MathF.PI * 2;

            var tc = MathF.Abs(tb);

            if (tc < MathF.PI / 4f)
                camera.AddConeFilled(tpos, radius, rot + 180.Degrees(), 45.Degrees(), 0x32000000 + (_uiConfig.RearColor.ABGR & 0xFFFFFF));

            else if (tc < MathF.PI * 3 / 4f)
                camera.AddConeFilled(tpos, radius, rot + (tb < 0 ? -90 : 90).Degrees(), 45.Degrees(), 0x32000000 + (_uiConfig.FlankColor.ABGR & 0xFFFFFF));
        }
    }

    void DrawPositionals(Actor player, Actor target, Positional pos, bool imminent, bool correct)
    {
        var radius = target.HitboxRadius + player.HitboxRadius + 3;

        var posColor = imminent ? (correct ? _uiConfig.PositionalGoodNow : _uiConfig.PositionalBadNow) : (correct ? _uiConfig.PositionalGoodLater : _uiConfig.PositionalBadLater);

        switch (pos)
        {
            case Positional.Rear:
                camera.AddCone(target.PosRot.XYZ(), radius, target.Rotation + 180.Degrees(), 45.Degrees(), posColor.ABGR, 2);
                break;
            case Positional.Flank:
                camera.AddCone(target.PosRot.XYZ(), radius, target.Rotation + 90.Degrees(), 45.Degrees(), posColor.ABGR, 2);
                camera.AddCone(target.PosRot.XYZ(), radius, target.Rotation - 90.Degrees(), 45.Degrees(), posColor.ABGR, 2);
                break;
        }
    }
}
