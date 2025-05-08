namespace BossMod.RealmReborn.Dungeon.D14Praetorium.D143Gaius;

public enum OID : uint
{
    Boss = 0x3875, // x1
    Helper = 0x233C, // x16
    PhantomGaiusSide = 0x3876, // x5, untargetable
    PhantomGaiusAdd = 0x3877, // x4, adds that become targetable on low hp
    TerminusEst = 0x3878, // spawn during fight
}

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
}

class TerminusEstTriple(BossModule module) : Components.StandardAOEs(module, AID.TerminusEstTriple, new AOEShapeRect(40, 2));
class TerminusEstQuintuple(BossModule module) : Components.StandardAOEs(module, AID.TerminusEstQuintuple, new AOEShapeRect(40, 2));
class HandOfTheEmpire(BossModule module) : Components.SpreadFromCastTargets(module, AID.HandOfTheEmpireAOE, 5, false);
class FestinaLente(BossModule module) : Components.StackWithCastTargets(module, AID.FestinaLente, 6, 4);
class Innocence(BossModule module) : Components.SingleTargetCast(module, AID.Innocence);
class HorridaBella(BossModule module) : Components.RaidwideCast(module, AID.HorridaBella);
class Ductus(BossModule module) : Components.StandardAOEs(module, AID.DuctusAOE, 8);

class AddEnrage(BossModule module) : BossComponent(module)
{
    private DateTime _enrage;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_enrage != default)
            hints.Add($"Enrage in {(_enrage - WorldState.CurrentTime).TotalSeconds:f1}s");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AddPhaseStart)
            _enrage = WorldState.FutureTime(91);
    }
}

class Heirsbane(BossModule module) : Components.SingleTargetCast(module, AID.Innocence, "");

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

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 16, NameID = 2136)]
public class D143Gaius(WorldState ws, Actor primary) : BossModule(ws, primary, new(-562, 220), new ArenaBoundsRect(15, 20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var add in Enemies(OID.PhantomGaiusAdd))
            Arena.Actor(add, ArenaColor.Enemy);
    }
}
