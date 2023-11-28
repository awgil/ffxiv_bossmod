using System.Linq;

namespace BossMod.Endwalker.Ultimate.TOP
{
    // TODO: not sure how exactly second target is selected, I think it is snapshotted to the current target when first cast happens? I can definitely taunt between casts without dying...
    // TODO: consider generalizing - same as P12S1 Glaukopis and others...
    class P5SolarRay : Components.GenericBaitAway
    {
        private static AOEShapeCircle _shape = new(5);

        public P5SolarRay() : base(centerAtTarget: true) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (CurrentBaits.Any(b => b.Source.TargetID == b.Target.InstanceID) && actor.Role == Role.Tank)
                hints.Add(module.PrimaryActor.TargetID != actor.InstanceID ? "Taunt!" : "Pass aggro!");
            base.AddHints(module, slot, actor, hints, movementHints);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.P5SolarRay)
            {
                var target = module.WorldState.Actors.Find(spell.TargetID);
                if (target != null)
                    CurrentBaits.Add(new(caster, target, _shape));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.P5SolarRay or AID.P5SolarRaySecond)
            {
                CurrentBaits.Clear();
                if (++NumCasts < 2 && module.WorldState.Actors.Find(spell.MainTargetID) is var target && target != null)
                    CurrentBaits.Add(new(caster, target, _shape));
            }
        }
    }
}
