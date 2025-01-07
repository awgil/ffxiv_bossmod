namespace BossMod.Stormblood.Quest.TheResonant;

public enum OID : uint
{
    Boss = 0x1B7D,
    Helper = 0x233C,
    _Gen_FordolaRemLupis = 0x18D6, // R0.500, x4, Helper type
    _Gen_MarkXLIIIArtilleryCannon = 0x1B7E, // R0.600, x0 (spawn during fight)
    _Gen_12ThLegionSignifer = 0x1B7B, // R0.500, x0 (spawn during fight)
    _Gen_12ThLegionLaquearius = 0x1B7C, // R0.500, x0 (spawn during fight)
    _Gen_12ThLegionHoplomachus = 0x1B7A, // R0.500, x0 (spawn during fight)
    _Gen_FordolaRemLupis1 = 0x1BCA, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 870, // Boss/1B7C/1B7A->player/1B6C/1B69/1B6B, no cast, single-target
    _Weaponskill_Innocence = 9105, // Boss->self, no cast, range 6+R ?-degree cone
    _Weaponskill_ResonatingEcho = 9113, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ = 8689, // 18D6->self, no cast, single-target
    _Weaponskill_Counter = 9103, // 18D6->player/1B6B, no cast, single-target
    _Weaponskill_PlanB = 8455, // Boss->self, 3.0s cast, single-target
    _Ability_MagitekRay = 9104, // 1B7E->self, 2.5s cast, range 45+R width 2 rect
    _Weaponskill_ChoppingBlock = 9109, // Boss->self, 2.3s cast, single-target
    _Weaponskill_ChoppingBlock1 = 9110, // 18D6->location, 3.0s cast, range 5 circle
    _Ability_ = 8338, // Boss->location, no cast, ???
    _Spell_Fire = 966, // 1B7B->1B69, 1.0s cast, single-target
    _Weaponskill_TerminusEst = 9107, // Boss->self, no cast, single-target
    _Weaponskill_TheOrder = 9106, // Boss->self, 5.0s cast, single-target
    _Weaponskill_HeavySwing = 8186, // _Gen_12ThLegionLaquearius->1B69/1B6C, no cast, single-target
    _Weaponskill_FastBlade = 717, // _Gen_12ThLegionHoplomachus->1B6B/1B69/1B6C, no cast, single-target
    _Weaponskill_TerminusEst1 = 9108, // _Gen_FordolaRemLupis1->self, no cast, range 40+R width 4 rect
    _Weaponskill_Skullbreaker = 9111, // Boss->self, 5.3s cast, single-target
    _Weaponskill_Skullbreaker1 = 9112, // _Gen_FordolaRemLupis->self, 6.0s cast, range 40 circle
}

public enum SID : uint
{
    Resonant = 780,
}

class Skullbreaker(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Skullbreaker1), new AOEShapeCircle(12));

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
        if (spell.Action.ID == (uint)AID._Weaponskill_TheOrder)
            Activation = Module.CastFinishAt(spell).AddSeconds(0.8f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (int)AID._Weaponskill_TerminusEst1)
            Activation = null;
    }
}
class MagitekRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_MagitekRay), new AOEShapeRect(45.6f, 1));
class ChoppingBlock(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_ChoppingBlock1), 5);

class Siphon(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
        {
            if (h.Actor.FindStatus(SID.Resonant) != null)
            {
                h.Priority = AIHints.Enemy.PriorityForbidden;
                hints.ActionsToExecute.Push(WorldState.Client.DutyActions[0].Action, h.Actor, ActionQueue.Priority.Medium);
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

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68086, NameID = 6104)]
public class FordolaRemLupis(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsSquare(19.5f));
