using System;
using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    // TODO: revise & generalize to 'baited aoe' component, with nice utilities for AI
    class Eruption : Components.LocationTargetedAOEs
    {
        public Eruption() : base(ActionID.MakeSpell(AID.EruptionAOE), 8) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);

            // TODO: better bait AI? currently, if actor is close to the center of any aoe, he is considered a 'potential baiter' and directed to move perpendicular to a boss...
            if (Casters.Any(c => c.CastInfo!.LocXZ.AlmostEqual(actor.Position, 1)))
            {
                var toBoss = (module.PrimaryActor.Position - module.Bounds.Center).Normalized();
                var actorOffset = actor.Position - module.Bounds.Center;
                var ortho = actorOffset - toBoss * toBoss.Dot(actorOffset);
                hints.AddForbiddenZone(ShapeDistance.Cone(actor.Position, 50, Angle.FromDirection(-ortho), 150.Degrees()), DateTime.MaxValue);
            }
        }
    }
}
