using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P1ProteanWaveTornadoVis : Components.SelfTargetedAOEs
    {
        public P1ProteanWaveTornadoVis() : base(ActionID.MakeSpell(AID.ProteanWaveTornadoVis), new AOEShapeCone(40, 15.Degrees()), true) { }
    }

    class P1ProteanWaveTornado : Components.CastCounter
    {
        private P1ProteanWaveTornadoVis? _casting;
        private List<(Actor Source, Actor Target, Angle Angle, BitMask Clipped)> _baited = new();
        private BitMask _clippedPlayers;

        private static AOEShapeCone _shape = new(40, 15.Degrees());

        public P1ProteanWaveTornado() : base(ActionID.MakeSpell(AID.ProteanWaveTornadoInvis)) { }

        public override void Init(BossModule module)
        {
            _casting = module.FindComponent<P1ProteanWaveTornadoVis>();
        }

        public override void Update(BossModule module)
        {
            _baited.Clear();
            _clippedPlayers = new();
            if (_casting?.ActiveCasters.Any() ?? false)
                return; // nothing is being baited while casts are in progress

            foreach (var tornado in module.Enemies(OID.LiquidRage))
            {
                var target = module.Raid.WithoutSlot().Closest(tornado.Position);
                if (target == null)
                    continue;

                var dir = Angle.FromDirection(target.Position - tornado.Position);
                var clipped = module.Raid.WithSlot().Exclude(target).InShape(_shape, tornado.Position, dir).Mask();
                _baited.Add((tornado, target, dir, clipped));
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
            foreach (var bait in _baited)
                _shape.Outline(arena, bait.Source.Position, bait.Angle);
        }
    }
}
