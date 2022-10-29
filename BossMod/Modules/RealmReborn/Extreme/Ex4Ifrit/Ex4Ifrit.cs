using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    class Incinerate : Components.Cleave
    {
        public static AOEShapeCone CleaveShape = new(21, 60.Degrees());

        public Incinerate() : base(ActionID.MakeSpell(AID.Incinerate), CleaveShape) { }

        // no-op, we provide custom positioning hints
        //public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
    }

    class RadiantPlume : Components.LocationTargetedAOEs
    {
        public RadiantPlume() : base(ActionID.MakeSpell(AID.RadiantPlumeAOE), 8) { }
    }

    // TODO: consider showing next charge before its cast starts...
    class CrimsonCyclone : Components.SelfTargetedAOEs
    {
        public CrimsonCyclone() : base(ActionID.MakeSpell(AID.CrimsonCyclone), new AOEShapeRect(49, 9)) { }
    }

    // note: boss has multiple phases triggered by hp, but we represent them as states to avoid problems associated with component deletion (TODO: reconsider...)
    class Ex4IfritStates : StateMachineBuilder
    {
        Ex4Ifrit _module;

        public Ex4IfritStates(Ex4Ifrit module) : base(module)
        {
            _module = module;
            DeathPhase(0, SinglePhase);
        }

        private void SinglePhase(uint id)
        {
            NailsSubphase<Ex4IfritAINails1, Ex4IfritAIHellfire1>(id, "4 nails at ~85%", false, false, 45);
            NailsSubphase<Ex4IfritAINails2, Ex4IfritAIHellfire2>(id + 0x10000, "8 nails at ~50%", true, true, 75);
            NailsSubphase<Ex4IfritAINails3, Ex4IfritAIHellfire3>(id + 0x20000, "13 nails at ~30%", true, false, 115);
            SimpleState(id + 0x30000, 1000, "Enrage")
                .ActivateOnEnter<Incinerate>()
                .ActivateOnEnter<Eruption>()
                .ActivateOnEnter<SearingWind>()
                .ActivateOnEnter<InfernalFetters>()
                .ActivateOnEnter<Ex4IfritAINormal>()
                .DeactivateOnExit<Ex4IfritAINormal>();
        }

        private void NailsSubphase<AINails, AIHellfire>(uint id, string name, bool withFetters, bool startWithOT, float nailEnrage)
            where AINails : Ex4IfritAINails, new()
            where AIHellfire : Ex4IfritAIHellfire, new()
        {
            Condition(id, 1000, () => _module.SmallNails.Any(a => a.IsTargetable && !a.IsDead), name)
                .ActivateOnEnter<Incinerate>()
                .ActivateOnEnter<Eruption>()
                .ActivateOnEnter<SearingWind>()
                .ActivateOnEnter<InfernalFetters>(withFetters)
                .ActivateOnEnter<Ex4IfritAINormal>()
                .OnEnter(() => Module.FindComponent<Ex4IfritAINormal>()!.BossTankRole = startWithOT ? PartyRolesConfig.Assignment.OT : PartyRolesConfig.Assignment.MT)
                .DeactivateOnExit<Incinerate>() // we want to reset cast counter
                .DeactivateOnExit<Ex4IfritAINormal>();
            Condition(id + 1, nailEnrage, () => !Module.PrimaryActor.IsTargetable, "Nails enrage", 1000)
                .ActivateOnEnter<Incinerate>()
                .ActivateOnEnter<AINails>()
                .DeactivateOnExit<Incinerate>()
                .DeactivateOnExit<Eruption>()
                .DeactivateOnExit<InfernalFetters>(withFetters)
                .DeactivateOnExit<AINails>()
                .SetHint(StateMachine.StateHint.DowntimeStart);

            CastStart(id + 0x10, AID.Hellfire, 3.4f)
                .ActivateOnEnter<Hellfire>()
                .ActivateOnEnter<AIHellfire>();
            CastEnd(id + 0x11, 2, "Raidwide")
                .DeactivateOnExit<Hellfire>()
                .SetHint(StateMachine.StateHint.Raidwide);
            Condition(id + 0x20, 5.1f, () => Module.PrimaryActor.FindStatus(SID.Invincibility) == null, "Invincible end", 10)
                .DeactivateOnExit<AIHellfire>()
                .SetHint(StateMachine.StateHint.DowntimeEnd);
            CastStart(id + 0x30, AID.RadiantPlume, 7.3f) // TODO: do we even need any AI here?..
                .ActivateOnEnter<Incinerate>()
                .DeactivateOnExit<Incinerate>(); // allow dodging plumes near mt
            CastEnd(id + 0x31, 2.2f)
                .ActivateOnEnter<RadiantPlume>();
            ComponentCondition<RadiantPlume>(id + 0x32, 0.9f, comp => comp.Casters.Count == 0, "Plumes")
                .DeactivateOnExit<RadiantPlume>();

            // note: cyclones could be skipped with high enough dps
            Condition(id + 0x40, 13.5f, () => !Module.PrimaryActor.IsTargetable || _module.SmallNails.Any(a => a.IsTargetable && !a.IsDead), "Disappear", 10)
                .ActivateOnEnter<Incinerate>()
                .ActivateOnEnter<Ex4IfritAINormal>()
                .DeactivateOnExit<Incinerate>()
                .DeactivateOnExit<Ex4IfritAINormal>();
            Targetable(id + 0x50, true, 15.4f, "Cyclones + Reappear")
                .ActivateOnEnter<CrimsonCyclone>()
                .DeactivateOnExit<CrimsonCyclone>();
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
