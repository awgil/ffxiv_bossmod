using System;

namespace BossMod.RealmReborn.Dungeon.D14Praetorium.D143Gaius
{
    public enum OID : uint
    {
        Boss = 0x3875, // x1
        Helper = 0x233C, // x16
        PhantomGaiusSide = 0x3876, // x5, untargetable
        PhantomGaiusAdd = 0x3877, // x4, adds that become targetable on low hp
        TerminusEst = 0x3878, // spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target

        Phantasmata = 28484, // Boss->self, 3.0s cast, single-target, visual (shows 'side' untargetable adds)
        PhantasmataShow = 28485, // PhantomGaiusSide/PhantomGaiusAdd->location, no cast, single-target (become visible)
        TerminusEstSummon = 28486, // Boss/PhantomGaiusSide->self, no cast, single-target, visual (show 'X' and spawn TerminusEst)
        TerminusEstTriple = 28487, // TerminusEst->self, 3.0s cast, range 40 width 4 rect, always three neighbouring casters
        TerminusEstQuintuple = 28488, // TerminusEst->self, 3.0s cast, range 40 width 4 rect, always five casters with gaps between
        TerminusEstVisual = 29779, // Helper->self, 6.0s cast, range 40 width 12 rect, no effect?..

        HandOfTheEmpire = 28491, // Boss/PhantomGaiusAdd->self, 3.0s cast, single-target, visual
        HandOfTheEmpireAOE = 28492, // Helper->player, 5.0s cast, range 5 circle spread
        FestinaLente = 28493, // Boss->player, 5.0s cast, range 6 circle stack
        Innocence = 28494, // Boss->player, 5.0s cast, single-target tankbuster
        HorridaBella = 28495, // Boss->self, 5.0s cast, raidwide
        Teleport = 28496, // Boss->location, no cast, single-target

        Ductus = 29051, // Boss/PhantomGaiusAdd->self, 3.0s cast, single-target, visual
        DuctusAOE = 29052, // Helper->location, 5.0s cast, range 8 circle aoe (pseudo exaflare)

        AddPhaseStart = 28497, // Boss->self, no cast, single-target, visual (enemy 'lb' gauge starts filling over 90 secs)
        Heirsbane = 28498, // PhantomGaiusAdd->player, 5.0s cast, single-target damage
        VeniVidiVici = 28499, // Boss->self, no cast, raidwide on last add death
        VeniVidiViciEnrage = 28500, // Boss->self, no cast, enrage (if adds aren't killed in 90s)
    };

    class TerminusEstTriple : Components.SelfTargetedAOEs
    {
        public TerminusEstTriple() : base(ActionID.MakeSpell(AID.TerminusEstTriple), new AOEShapeRect(40, 2)) { }
    }

    class TerminusEstQuintuple : Components.SelfTargetedAOEs
    {
        public TerminusEstQuintuple() : base(ActionID.MakeSpell(AID.TerminusEstQuintuple), new AOEShapeRect(40, 2)) { }
    }

    class HandOfTheEmpire : Components.SpreadFromCastTargets
    {
        public HandOfTheEmpire() : base(ActionID.MakeSpell(AID.HandOfTheEmpireAOE), 5, false) { }
    }

    class FestinaLente : Components.StackWithCastTargets
    {
        public FestinaLente() : base(ActionID.MakeSpell(AID.FestinaLente), 6, 4) { }
    }

    class Innocence : Components.SingleTargetCast
    {
        public Innocence() : base(ActionID.MakeSpell(AID.Innocence)) { }
    }

    class HorridaBella : Components.RaidwideCast
    {
        public HorridaBella() : base(ActionID.MakeSpell(AID.HorridaBella)) { }
    }

    class Ductus : Components.LocationTargetedAOEs
    {
        public Ductus() : base(ActionID.MakeSpell(AID.DuctusAOE), 8) { }
    }

    class AddEnrage : BossComponent
    {
        private DateTime _enrage;

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_enrage != new DateTime())
                hints.Add($"Enrage in {(_enrage - module.WorldState.CurrentTime).TotalSeconds:f1}s");
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.AddPhaseStart)
                _enrage = module.WorldState.CurrentTime.AddSeconds(91);
        }
    }

    class Heirsbane : Components.SingleTargetCast
    {
        public Heirsbane() : base(ActionID.MakeSpell(AID.Innocence), "") { }
    }

    class D143GaiusStates : StateMachineBuilder
    {
        public D143GaiusStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<TerminusEstTriple>()
                .ActivateOnEnter<TerminusEstQuintuple>()
                .ActivateOnEnter<HandOfTheEmpire>()
                .ActivateOnEnter<FestinaLente>()
                .ActivateOnEnter<Innocence>()
                .ActivateOnEnter<HorridaBella>()
                .ActivateOnEnter<Ductus>()
                .ActivateOnEnter<AddEnrage>()
                .ActivateOnEnter<Heirsbane>();
        }
    }

    public class D143Gaius : BossModule
    {
        public D143Gaius(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-562, 220), 15, 20)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var add in Enemies(OID.PhantomGaiusAdd))
                Arena.Actor(add, ArenaColor.Enemy);
        }
    }
}
