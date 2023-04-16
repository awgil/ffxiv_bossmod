using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P2Flarethrower : Components.GenericBaitAway
    {
        private Actor? _source;

        private static AOEShapeCone _shape = new(100, 45.Degrees()); // TODO: verify angle

        public P2Flarethrower() : base(ActionID.MakeSpell(AID.FlarethrowerP2AOE)) { }

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            if (_source != null && module.Raid.WithoutSlot().Closest(_source.Position)  is var target && target != null)
                CurrentBaits.Add(new(_source, target, _shape));
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (_source != null && CurrentBaits.Any(b => b.Target == actor) && module.Enemies(OID.LiquidRage).Any(r => !_shape.Check(r.Position, _source.Position, Angle.FromDirection(actor.Position - _source.Position))))
                hints.Add("Aim towards tornado!");
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.FlarethrowerP2)
            {
                _source = caster;
                ForbiddenPlayers = module.Raid.WithSlot(true).WhereActor(a => a.InstanceID != caster.TargetID).Mask(); // TODO: unsure about this... assumes BJ main target should bait
            }
        }
    }
}
