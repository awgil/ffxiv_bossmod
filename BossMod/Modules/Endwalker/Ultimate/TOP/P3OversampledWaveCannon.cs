using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class P3OversampledWaveCannon : BossComponent
    {
        private Actor? _boss;
        private Angle _bossAngle;
        private Angle[] _playerAngles = new Angle[PartyState.MaxPartySize];
        private int[] _playerOrder = new int[PartyState.MaxPartySize];
        private int _numPlayerAngles;
        private List<int> _monitorOrder = new();

        private static AOEShapeRect _shape = new(50, 50);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_playerOrder[slot] != 0)
                hints.Add($"Order: {(IsMonitor(slot) != default ? "M" : "N")}{_playerOrder[slot]}", false);

            var numHitBy = AOEs(module, slot).Count(a => !a.source && _shape.Check(actor.Position, a.origin, a.rot));
            if (numHitBy != 1)
                hints.Add($"Hit by {numHitBy} monitors!");

            if (movementHints != null)
                foreach (var p in SafeSpots(module, slot).Where(p => p.assigned))
                    movementHints.Add(actor.Position, p.pos, ArenaColor.Safe);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var a in AOEs(module, pcSlot))
                _shape.Draw(arena, a.origin, a.rot, a.safe ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in SafeSpots(module, pcSlot))
                arena.AddCircle(p.pos, 1, p.assigned ? ArenaColor.Safe : ArenaColor.Danger);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var angle = (SID)status.ID switch
            {
                SID.OversampledWaveCannonLoadingL => 90.Degrees(),
                SID.OversampledWaveCannonLoadingR => -90.Degrees(),
                _ => default
            };
            if (angle != default && module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            {
                _playerAngles[slot] = angle;
                if (++_numPlayerAngles == 3)
                {
                    int n = 0, m = 0;
                    foreach (var sg in Service.Config.Get<TOPConfig>().P3MonitorsAssignments.Resolve(module.Raid).OrderBy(sg => sg.group))
                    {
                        _playerOrder[sg.slot] = IsMonitor(sg.slot) ? ++m : ++n;
                        if (IsMonitor(sg.slot))
                            _monitorOrder.Add(sg.slot);
                    }
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var angle = (AID)spell.Action.ID switch
            {
                AID.OversampledWaveCannonL => 90.Degrees(),
                AID.OversampledWaveCannonR => -90.Degrees(),
                _ => default
            };
            if (angle != default)
            {
                _boss = caster;
                _bossAngle = angle;
            }
        }

        private bool IsMonitor(int slot) => _playerAngles[slot] != default;

        private IEnumerable<(WPos pos, bool assigned)> SafeSpots(BossModule module, int slot)
        {
            if (_numPlayerAngles < 3 || _bossAngle == default)
                yield break;

            WPos adjust(float x, float z) => module.Bounds.Center + new WDir(_bossAngle.Rad < 0 ? -x : x, z);
            if (IsMonitor(slot))
            {
                yield return (adjust(10, -11), _playerOrder[slot] == 1);
                yield return (adjust(-11, -9), _playerOrder[slot] == 2);
                yield return (adjust(-11, +9), _playerOrder[slot] == 3);
            }
            else
            {
                yield return (adjust(1, -15), _playerOrder[slot] == 1);
                yield return (adjust(15, -4), _playerOrder[slot] == 2);
                yield return (adjust(15, +4), _playerOrder[slot] == 3);
                yield return (adjust(10, 11), _playerOrder[slot] == 4);
                yield return (adjust(1, 15), _playerOrder[slot] == 5);
            }
        }

        private IEnumerable<(WPos origin, Angle rot, bool safe, bool source)> AOEs(BossModule module, int slot)
        {
            var isMonitor = IsMonitor(slot);
            var order = (isMonitor, _playerOrder[slot]) switch
            {
                (_, 1) => 2, // N1/M1 are hit by M2
                (true, _) => 0, // M2/M3 are hit by boss
                (_, 2 or 3) => 1, // N2/N3 are hit by M1
                _ => 3, // N4/N5 are hit by M3
            };
            foreach (var aoe in AOEs(module))
                if (aoe.origin != null)
                    yield return (aoe.origin.Position, aoe.origin.Rotation + aoe.offset, aoe.order == order, isMonitor && aoe.order == _playerOrder[slot]);
        }

        private IEnumerable<(Actor? origin, Angle offset, int order)> AOEs(BossModule module)
        {
            yield return (_boss, _bossAngle, 0);
            for (int i = 0; i < _monitorOrder.Count; ++i)
            {
                var slot = _monitorOrder[i];
                yield return (module.Raid[slot], _playerAngles[slot], i + 1);
            }
        }
    }

    class P3OversampledWaveCannonSpread : Components.UniformStackSpread
    {
        public P3OversampledWaveCannonSpread() : base(0, 7) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.OversampledWaveCannonR or AID.OversampledWaveCannonL)
                AddSpreads(module.Raid.WithoutSlot(true), spell.NPCFinishAt);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.OversampledWaveCannonAOE)
                Spreads.Clear();
        }
    }
}
