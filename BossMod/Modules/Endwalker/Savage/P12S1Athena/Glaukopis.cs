using System.Linq;

namespace BossMod.Endwalker.Savage.P12S1Athena
{
    // TODO: not sure how exactly second target is selected, I think it is snapshotted to the current target when first cast happens? I can definitely taunt between casts without dying...
    // TODO: consider generalizing...
    class Glaukopis : Components.GenericBaitAway
    {
        private static AOEShapeRect _shape = new(60, 2.5f);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (CurrentBaits.Any(b => b.Source.TargetID == b.Target.InstanceID) && actor.Role == Role.Tank)
                hints.Add(module.PrimaryActor.TargetID != actor.InstanceID ? "Taunt!" : "Pass aggro!");
            base.AddHints(module, slot, actor, hints, movementHints);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Glaukopis)
            {
                var target = module.WorldState.Actors.Find(spell.TargetID);
                if (target != null)
                    CurrentBaits.Add(new(caster, target, _shape));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.Glaukopis or AID.GlaukopisSecond)
            {
                CurrentBaits.Clear();
                if (++NumCasts < 2 && module.WorldState.Actors.Find(caster.TargetID) is var target && target != null)
                    CurrentBaits.Add(new(caster, target, _shape));
            }
        }
    }
}
