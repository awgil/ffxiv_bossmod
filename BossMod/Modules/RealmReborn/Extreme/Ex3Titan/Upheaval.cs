using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex3Titan
{
    class Upheaval : Components.KnockbackFromCaster
    {
        public Upheaval() : base(ActionID.MakeSpell(AID.Upheaval), 13) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var caster = ActiveCasters.FirstOrDefault();
            if (caster != null)
            {
                // stack just behind boss, this is a good place to bait imminent landslide correctly
                var pos = module.PrimaryActor.Position + new WDir(0, 2);
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pos, 1.5f), caster.CastInfo!.FinishAt);
            }
        }
    }
}
