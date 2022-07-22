using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P1ProteanWaveLiquidVisBoss : Components.SelfTargetedAOEs
    {
        public P1ProteanWaveLiquidVisBoss() : base(ActionID.MakeSpell(AID.ProteanWaveLiquidVisBoss), new AOEShapeCone(40, 15.Degrees())) { }
    }

    class P1ProteanWaveLiquidVisHelper : Components.SelfTargetedAOEs
    {
        public P1ProteanWaveLiquidVisHelper() : base(ActionID.MakeSpell(AID.ProteanWaveLiquidVisHelper), new AOEShapeCone(40, 15.Degrees())) { }
    }

    class P1ProteanWaveLiquidInvis : Components.CastCounter
    {
        private Actor? _source;
        private List<(Actor Target, Angle Angle, BitMask Clipped)> _baited = new();
        private BitMask _clippedPlayers;

        private static AOEShapeCone _shape = new(40, 15.Degrees());

        public P1ProteanWaveLiquidInvis() : base(ActionID.MakeSpell(AID.ProteanWaveLiquidInvisBoss)) { }

        public override void Init(BossModule module)
        {
            _source = module.Enemies(OID.BossP1).FirstOrDefault();
        }

        public override void Update(BossModule module)
        {
            _baited.Clear();
            if (_source == null)
                return;
            _clippedPlayers = module.Raid.WithSlot().InShape(_shape, _source).Mask();
            foreach (var target in module.Raid.WithoutSlot().SortedByRange(_source.Position).Take(4))
            {
                var dir = Angle.FromDirection(target.Position - _source.Position);
                var clipped = module.Raid.WithSlot().Exclude(target).InShape(_shape, _source.Position, dir).Mask();
                _baited.Add((target, dir, clipped));
                _clippedPlayers |= clipped;
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_clippedPlayers[slot])
            {
                hints.Add("GTFO from baited protean!");
            }

            var index = _baited.FindIndex(x => x.Target == actor);
            if (index >= 0 && _baited[index].Clipped.Any())
            {
                hints.Add("Aim protean away from raid!");
            }
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            if (_baited.Any(x => x.Target == player))
                return PlayerPriority.Danger;
            if (_clippedPlayers[playerSlot])
                return PlayerPriority.Interesting;
            return PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_source == null)
                return;
            _shape.Draw(arena, _source);
            foreach (var bait in _baited)
                _shape.Draw(arena, _source.Position, bait.Angle);
        }
    }
}
