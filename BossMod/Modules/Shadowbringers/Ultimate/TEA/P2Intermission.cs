using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    // TODO: show various knockbacks hint
    class P2Intermission : BossComponent
    {
        public int NumCasts { get; private set; } = 0;
        private Angle _blasterStartingDirection;
        private int[] _playerOrder = new int[PartyState.MaxPartySize];

        private static float _blasterOffset = 14;
        private static AOEShapeCircle _blasterShape = new(10);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_playerOrder[slot] > 0)
                hints.Add($"Order: {_playerOrder[slot]}", false);
            if (ImminentBlasterCenters(module).Any(c => _blasterShape.Check(actor.Position, c)))
                hints.Add("GTFO from aoe!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in FutureBlasterCenters(module))
                _blasterShape.Draw(arena, c, default);
            foreach (var c in ImminentBlasterCenters(module))
                _blasterShape.Draw(arena, c, default, ArenaColor.Danger);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.HawkBlasterIntermission:
                    if (NumCasts == 0)
                        _blasterStartingDirection = Angle.FromDirection(spell.TargetXZ - module.Bounds.Center);
                    ++NumCasts;
                    break;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID >= 79 && iconID <= 86)
            {
                int slot = module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    _playerOrder[slot] = (int)iconID - 78;
            }
        }

        // 0,1,2,3 - offset aoes, 4 - center aoe
        private int NextBlasterIndex => NumCasts switch
        {
            0 or 1 => 0,
            2 or 3 => 1,
            4 or 5 => 2,
            6 or 7 => 3,
            8 => 4,
            9 or 10 => 5,
            11 or 12 => 6,
            13 or 14 => 7,
            15 or 16 => 8,
            17 => 9,
            _ => 10
        };

        private IEnumerable<WPos> BlasterCenters(BossModule module, int index)
        {
            switch (index)
            {
                case 0: case 1: case 2: case 3:
                    {
                        var dir = (_blasterStartingDirection - index * 45.Degrees()).ToDirection();
                        yield return module.Bounds.Center + _blasterOffset * dir;
                        yield return module.Bounds.Center - _blasterOffset * dir;
                    }
                    break;
                case 5: case 6: case 7: case 8:
                    {
                        var dir = (_blasterStartingDirection - (index - 5) * 45.Degrees()).ToDirection();
                        yield return module.Bounds.Center + _blasterOffset * dir;
                        yield return module.Bounds.Center - _blasterOffset * dir;
                    }
                    break;
                case 4: case 9:
                    yield return module.Bounds.Center;
                    break;
            }
        }

        private IEnumerable<WPos> ImminentBlasterCenters(BossModule module) => NumCasts > 0 ? BlasterCenters(module, NextBlasterIndex) : Enumerable.Empty<WPos>();
        private IEnumerable<WPos> FutureBlasterCenters(BossModule module) => NumCasts > 0 ? BlasterCenters(module, NextBlasterIndex + 1) : Enumerable.Empty<WPos>();
    }
}
