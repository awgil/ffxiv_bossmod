using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UCOB
{
    class P2Heavensfall : Components.Knockback
    {
        public P2Heavensfall() : base(ActionID.MakeSpell(AID.Heavensfall), true) { }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            yield return new(module.Bounds.Center, 11); // TODO: activation
        }
    }

    class P2ThermionicBurst : Components.SelfTargetedAOEs
    {
        public P2ThermionicBurst() : base(ActionID.MakeSpell(AID.ThermionicBurst), new AOEShapeCone(24.5f, 11.25f.Degrees())) { }
    }

    class P2MeteorStream : Components.UniformStackSpread
    {
        public int NumCasts;

        public P2MeteorStream() : base(0, 4, alwaysShowSpreads: true) { }

        public override void Init(BossModule module)
        {
            AddSpreads(module.Raid.WithoutSlot(true), module.WorldState.CurrentTime.AddSeconds(5.6f));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.MeteorStream)
            {
                ++NumCasts;
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            }
        }
    }

    class P2HeavensfallDalamudDive : Components.GenericBaitAway
    {
        private Actor? _target;

        private static AOEShapeCircle _shape = new(5);

        public P2HeavensfallDalamudDive() : base(ActionID.MakeSpell(AID.DalamudDive), true, true) { }

        public void Show()
        {
            if (_target != null)
            {
                CurrentBaits.Add(new(_target, _target, _shape));
            }
        }

        public override void Init(BossModule module)
        {
            _target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);
        }
    }
}
