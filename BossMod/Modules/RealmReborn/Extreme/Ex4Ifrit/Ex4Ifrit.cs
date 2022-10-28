using System.Collections.Generic;

namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    class Incinerate : Components.Cleave
    {
        public Incinerate() : base(ActionID.MakeSpell(AID.Incinerate), new AOEShapeCone(21, 60.Degrees())) { } // TODO: verify angle

        // no-op, we provide custom positioning hints
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
    }

    class RadiantPlume : Components.LocationTargetedAOEs
    {
        public RadiantPlume() : base(ActionID.MakeSpell(AID.RadiantPlumeAOE), 8) { }
    }

    class CrimsonCyclone : Components.SelfTargetedAOEs
    {
        public CrimsonCyclone() : base(ActionID.MakeSpell(AID.CrimsonCyclone), new AOEShapeRect(49, 9)) { }
    }

    class Ex4IfritStates : StateMachineBuilder
    {
        public Ex4IfritStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Incinerate>()
                .ActivateOnEnter<SearingWind>()
                .ActivateOnEnter<Eruption>()
                .ActivateOnEnter<Hellfire>()
                .ActivateOnEnter<RadiantPlume>()
                .ActivateOnEnter<CrimsonCyclone>()
                .ActivateOnEnter<InfernalFetters>()
                .ActivateOnEnter<Ex4IfritAI>();
        }
    }

    public class Ex4Ifrit : BossModule
    {
        public List<Actor> SmallNails;
        public List<Actor> LargeNails;

        public Ex4Ifrit(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 20))
        {
            SmallNails = Enemies(OID.InfernalNailSmall);
            LargeNails = Enemies(OID.InfernalNailLarge);
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actors(SmallNails, ArenaColor.Object);
            Arena.Actors(LargeNails, ArenaColor.Object);
        }
    }
}
