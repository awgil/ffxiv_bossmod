namespace BossMod.Heavensward.Quest.CloseEncountersOfTheVIthKind;

public enum OID : uint
{
    Boss = 0xF1C, // R0.550, x?
    Puddle = 0x1E88F5, // R0.500, x?
    TerminusEst = 0xF5D, // R1.000, x?
}

public enum AID : uint
{
    HandOfTheEmpire = 4000, // Boss->location, 2.0s cast, range 2 circle
    TerminusEstBoss = 4005, // Boss->self, 3.0s cast, range 50 circle
    TerminusEstAOE = 3825, // TerminusEst->self, no cast, range 40+R width 4 rect
}

class RegulaVanHydrusStates : StateMachineBuilder
{
    public RegulaVanHydrusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<Voidzone>()
            .ActivateOnEnter<HandOfTheEmpire>()
            ;
    }
}

class HandOfTheEmpire(BossModule module) : Components.StandardAOEs(module, AID.HandOfTheEmpire, 2);

class Voidzone(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.Puddle));

class TerminusEst(BossModule module) : Components.GenericAOEs(module, AID.TerminusEstAOE)
{
    private bool _active;

    private IEnumerable<Actor> Adds => Module.Enemies(OID.TerminusEst).Where(x => !x.IsDead);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Adds, ArenaColor.Danger, true);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => _active ? Adds.Select(x => new AOEInstance(new AOEShapeRect(40, 2), x.Position, x.Rotation)) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TerminusEstBoss)
            _active = true;
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.TerminusEst)
            _active = false;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67203, NameID = 3818)]
public class RegulaVanHydrus(WorldState ws, Actor primary) : BossModule(ws, primary, new(252.75f, 553), new ArenaBoundsCircle(19.5f));
