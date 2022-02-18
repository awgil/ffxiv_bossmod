using System.Numerics;

namespace BossMod.P2S
{
    using static BossModule;

    // state related to channeling [over]flow mechanics
    // TODO: improve (hint if too close to or missing a partner at very least...)
    class ChannelingFlow : Component
    {
        private P2S _module;

        private static float _typhoonHalfWidth = 2.5f;

        public ChannelingFlow(P2S module)
        {
            _module = module;
        }

        public override void DrawArenaBackground(MiniArena arena)
        {
            foreach (var player in _module.RaidMembers.WithoutSlot())
            {
                foreach (var status in player.Statuses)
                {
                    if ((status.ExpireAt - _module.WorldState.CurrentTime).TotalSeconds > 10)
                        continue;

                    switch ((SID)status.ID)
                    {
                        case SID.MarkFlowN:
                            arena.ZoneQuad(player.Position, -Vector3.UnitZ, 50, 0, _typhoonHalfWidth, arena.ColorAOE);
                            break;
                        case SID.MarkFlowS:
                            arena.ZoneQuad(player.Position, Vector3.UnitZ, 50, 0, _typhoonHalfWidth, arena.ColorAOE);
                            break;
                        case SID.MarkFlowW:
                            arena.ZoneQuad(player.Position, -Vector3.UnitX, 50, 0, _typhoonHalfWidth, arena.ColorAOE);
                            break;
                        case SID.MarkFlowE:
                            arena.ZoneQuad(player.Position, Vector3.UnitX, 50, 0, _typhoonHalfWidth, arena.ColorAOE);
                            break;
                    }
                }
            }
        }
    }
}
