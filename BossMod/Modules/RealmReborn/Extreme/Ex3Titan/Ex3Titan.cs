using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex3Titan
{
    class WeightOfTheLand : Components.LocationTargetedAOEs
    {
        public WeightOfTheLand() : base(ActionID.MakeSpell(AID.WeightOfTheLandAOE), 6) { }
    }

    class GaolerVoidzone : Components.PersistentVoidzone
    {
        public GaolerVoidzone() : base(5, m => m.Enemies(OID.GaolerVoidzone).Where(e => e.EventState != 7)) { }
    }

    [ConfigDisplay(Order = 030, Parent = typeof(RealmRebornConfig))]
    public class Ex3TitanConfig : CooldownPlanningConfigNode
    {
        public Ex3TitanConfig() : base(50) { }
    }

    public class Ex3Titan : BossModule
    {
        private List<Actor> _heart;
        public Actor? Heart() => _heart.FirstOrDefault();

        public List<Actor> Gaolers;
        public List<Actor> Gaols;
        public List<Actor> Bombs;

        public Ex3Titan(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 25))
        {
            _heart = Enemies(OID.TitansHeart);
            Gaolers = Enemies(OID.GraniteGaoler);
            Gaols = Enemies(OID.GraniteGaol);
            Bombs = Enemies(OID.BombBoulder);
        }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var heart = Heart();
            if (heart != null && heart.IsTargetable)
            {
                // heart is not added by default, since it has weird actor type
                // boss is not really a valid target, but it still hits tank pretty hard, so we want to set attacker strength (?)
                hints.PotentialTargets.Add(new(heart, false));
                //hints.PotentialTargets.Add(new(PrimaryActor, false));
            }
            base.CalculateAIHints(slot, actor, assignment, hints);
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            Arena.Actors(Gaolers, ArenaColor.Enemy);
            Arena.Actors(Gaols, ArenaColor.Object);
            Arena.Actors(Bombs, ArenaColor.Object);
        }
    }
}
