using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal class OverlayService(Camera camera, IObjectTable objectTable, IFramework framework, ConfigRoot configRoot) : IHostedService
{
    private readonly DtrConfig _uiConfig = configRoot.Get<DtrConfig>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        framework.Update += Draw;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        framework.Update -= Draw;
    }

    private void Draw(IFramework framework)
    {
        if (objectTable.LocalPlayer is not { } player)
            return;

        if (player.TargetObject is not { } target)
            return;

        if (_uiConfig.ShowMeleeRing)
        {
            var radius = target.HitboxRadius + player.HitboxRadius + 3;
            var rot = target.Rotation.Radians();

            camera.AddArc(target.Position, radius, rot + 180.Degrees(), 45.Degrees(), _uiConfig.RearColor.ABGR, 2);
            camera.AddArc(target.Position, radius, rot + 90.Degrees(), 45.Degrees(), _uiConfig.FlankColor.ABGR, 2);
            camera.AddArc(target.Position, radius, rot - 90.Degrees(), 45.Degrees(), _uiConfig.FlankColor.ABGR, 2);
            camera.AddArc(target.Position, radius, rot, 45.Degrees(), _uiConfig.FrontColor.ABGR, 2);

            if (_uiConfig.HighlightSlice)
            {
                var tt = target.Position - player.Position;
                var ta = MathF.Atan2(tt.X, tt.Z);
                var tb = target.Rotation - ta;
                while (tb > MathF.PI)
                    tb -= MathF.PI * 2;
                while (tb < -MathF.PI)
                    tb += MathF.PI * 2;

                var tc = MathF.Abs(tb);

                if (tc < MathF.PI / 4f)
                    camera.AddConeFilled(target.Position, radius, rot + 180.Degrees(), 45.Degrees(), 0x32000000 + (_uiConfig.RearColor.ABGR & 0xFFFFFF), 2);

                else if (tc < MathF.PI * 3 / 4f)
                    camera.AddConeFilled(target.Position, radius, rot + (tb < 0 ? -90 : 90).Degrees(), 45.Degrees(), 0x32000000 + (_uiConfig.FlankColor.ABGR & 0xFFFFFF), 2);

                //Service.Log($"{tb}");
            }
        }
    }
}
