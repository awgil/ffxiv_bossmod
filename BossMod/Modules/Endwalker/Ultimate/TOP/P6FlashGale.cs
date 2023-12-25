using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class P6FlashGale : Components.GenericBaitAway
    {
        private Actor? _source;

        private static AOEShapeCircle _shape = new(5);

        public P6FlashGale() : base(centerAtTarget: true) { }

        public override void Init(BossModule module)
        {
            _source = module.Enemies(OID.BossP6).FirstOrDefault();
            ForbiddenPlayers = module.Raid.WithSlot(true).WhereActor(p => p.Role != Role.Tank).Mask();
        }

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            if (_source != null)
            {
                var mainTarget = module.WorldState.Actors.Find(_source.TargetID);
                var farTarget = module.Raid.WithoutSlot().Farthest(_source.Position);
                if (mainTarget != null)
                    CurrentBaits.Add(new(_source, mainTarget, _shape));
                if (farTarget != null)
                    CurrentBaits.Add(new(_source, farTarget, _shape));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.FlashGale)
                ++NumCasts;
        }
    }
}
