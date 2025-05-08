namespace BossMod.Stormblood.Quest.TheOrphansAndTheBrokenBlade;

public enum OID : uint
{
    Boss = 0x1C5E,
    Helper = 0x233C,
}

public enum AID : uint
{
    ShadowOfDeath1 = 8459, // 1C5F->location, 3.0s cast, range 5 circle
    HeadsmansDelight = 8457, // Boss->1C5C, 5.0s cast, range 5 circle
    SpiralHell = 8453, // 1C5F->self, 3.0s cast, range 40+R width 4 rect
    HeadmansDelight = 9298, // 1C5F->player/1C5C, no cast, single-target
}

class SpiralHell(BossModule module) : Components.StandardAOEs(module, AID.SpiralHell, new AOEShapeRect(40, 2));
class HeadsmansDelight(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeadsmansDelight && WorldState.Actors.Find(spell.TargetID) is Actor tar)
            Stacks.Add(new(tar, 5, activation: Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HeadmansDelight)
            Stacks.Clear();
    }
}
class ShadowOfDeath(BossModule module) : Components.StandardAOEs(module, AID.ShadowOfDeath1, 5);
class DarkChain(BossModule module) : Components.Adds(module, 0x1C60)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(0x1C60, 5);
    }
}

class OmpagneDeepblackStates : StateMachineBuilder
{
    public OmpagneDeepblackStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShadowOfDeath>()
            .ActivateOnEnter<DarkChain>()
            .ActivateOnEnter<HeadsmansDelight>()
            .ActivateOnEnter<SpiralHell>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68453, NameID = 6300)]
public class OmpagneDeepblack(WorldState ws, Actor primary) : BossModule(ws, primary, new(-166.8f, 290), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
