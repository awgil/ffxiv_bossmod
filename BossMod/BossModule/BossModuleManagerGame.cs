using ImGuiNET;
using System;
using System.Numerics;

namespace BossMod
{
    // TODO: remove
    class BossModuleManagerGame : BossModuleManager
    {
        public BossModuleManagerGame(WorldState ws)
            : base(ws)
        {
        }

        protected override float PrepullTimer() => Countdown.TimeRemaining() ?? 10000;
    }
}
