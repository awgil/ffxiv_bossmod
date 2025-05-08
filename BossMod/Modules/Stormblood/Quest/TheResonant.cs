namespace BossMod.Stormblood.Quest.TheResonant;

public enum OID : uint
{
    Boss = 0x1B7D,
    Helper = 0x233C,
    FordolaRemLupis = 0x18D6, // R0.500, x4, Helper type
    MarkXLIIIArtilleryCannon = 0x1B7E, // R0.600, x0 (spawn during fight)
    FordolaRemLupis1 = 0x1BCA, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    MagitekRay = 9104, // 1B7E->self, 2.5s cast, range 45+R width 2 rect
    ChoppingBlock1 = 9110, // 18D6->location, 3.0s cast, range 5 circle
    TheOrder = 9106, // Boss->self, 5.0s cast, single-target
    TerminusEst1 = 9108, // FordolaRemLupis1->self, no cast, range 40+R width 4 rect
    Skullbreaker1 = 9112, // FordolaRemLupis->self, 6.0s cast, range 40 circle
}

public enum SID : uint
{
    Resonant = 780,
}

class Skullbreaker(BossModule module) : Components.StandardAOEs(module, AID.Skullbreaker1, new AOEShapeCircle(12));

class TerminusEst(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime? Activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Activation == null)
            yield break;

        var casters = Module.Enemies(0x1BCA).Where(e => e.Position.AlmostEqual(Arena.Center, 0.5f));
        foreach (var c in casters)
            yield return new AOEInstance(new AOEShapeRect(41, 2), c.Position, c.Rotation, Activation: Activation.Value);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheOrder)
            Activation = Module.CastFinishAt(spell).AddSeconds(0.8f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (int)AID.TerminusEst1)
            Activation = null;
    }
}
class MagitekRay(BossModule module) : Components.StandardAOEs(module, AID.MagitekRay, new AOEShapeRect(45.6f, 1));
class ChoppingBlock(BossModule module) : Components.StandardAOEs(module, AID.ChoppingBlock1, 5);

class Siphon(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
        {
            if (h.Actor.FindStatus(SID.Resonant) != null)
            {
                h.Priority = AIHints.Enemy.PriorityForbidden;
                hints.ActionsToExecute.Push(WorldState.Client.DutyActions[0].Action, h.Actor, ActionQueue.Priority.ManualEmergency); // use emergency mode to bypass forbidden state - duty action is the only thing we can use on fordola without being stunned
            }
        }
    }
}

public class FordolaRemLupisStates : StateMachineBuilder
{
    public FordolaRemLupisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Siphon>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<ChoppingBlock>()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<Skullbreaker>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68086, NameID = 6104)]
public class FordolaRemLupis(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsSquare(19.5f));
