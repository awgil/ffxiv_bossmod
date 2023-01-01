using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5TrinityAvowed
{
    class FreedomOfBozja : TemperatureAOE
    {
        private List<(Actor orb, int temperature)> _orbs = new();
        private DateTime _activation;
        private bool _risky;

        private static AOEShapeCircle _shape = new(22);

        public FreedomOfBozja(bool risky)
        {
            _risky = risky;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            var playerTemp = Temperature(actor);
            foreach (var o in _orbs)
                yield return new(_shape, o.orb.Position, o.orb.Rotation, _activation, o.temperature == -playerTemp ? ArenaColor.SafeFromAOE : ArenaColor.AOE, _risky);
        }

        public override void Init(BossModule module)
        {
            InitOrb(module, OID.SwirlingOrb, -1);
            InitOrb(module, OID.TempestuousOrb, -2);
            InitOrb(module, OID.BlazingOrb, +1);
            InitOrb(module, OID.RoaringOrb, +2);
            _activation = module.WorldState.CurrentTime.AddSeconds(10);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.ChillBlast1 or AID.FreezingBlast1 or AID.HeatedBlast1 or AID.SearingBlast1 or AID.ChillBlast2 or AID.FreezingBlast2 or AID.HeatedBlast2 or AID.SearingBlast2)
                ++NumCasts;
        }

        public bool ActorUnsafeAt(Actor actor, WPos pos)
        {
            var playerTemp = Temperature(actor);
            return _orbs.Any(o => _shape.Check(pos, o.orb.Position) != (o.temperature == -playerTemp));
        }

        private void InitOrb(BossModule module, OID oid, int temp)
        {
            var orb = module.Enemies(oid).FirstOrDefault();
            if (orb != null)
                _orbs.Add((orb, temp));
        }
    }

    class FreedomOfBozja1 : FreedomOfBozja
    {
        public FreedomOfBozja1() : base(false) { }
    }

    class QuickMarchStaff1 : QuickMarch
    {
        private FreedomOfBozja1? _freedom;

        public override void Init(BossModule module) => _freedom = module.FindComponent<FreedomOfBozja1>();

        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => !module.Bounds.Contains(pos) || (_freedom?.ActorUnsafeAt(actor, pos) ?? false);
    }

    class FreedomOfBozja2 : FreedomOfBozja
    {
        public FreedomOfBozja2() : base(true) { }
    }
}
