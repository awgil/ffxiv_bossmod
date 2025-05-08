using BossMod.QuestBattle.Endwalker.MSQ;

namespace BossMod.Endwalker.Quest.AsTheHeavensBurn.P1TerminusIdolizer;

public enum OID : uint
{
    Boss = 0x35E9,
    Helper = 0x233C,
    TerminusDetonator = 0x35E5, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    DeadlyCharge = 26995, // Boss->location, 5.0s cast, width 10 rect charge
    GriefOfParting = 26996, // Boss->self, 5.0s cast, range 40 circle
    DeadlyTentacles = 26997, // Boss->35F5, 5.0s cast, single-target
    TentacleWhipLFirst = 27004, // Boss->self, 5.0s cast, range 60 180-degree cone
    TentacleWhipRSecond = 27006, // Helper->self, 7.0s cast, range 60 180-degree cone
    SelfDestruct = 26991, // TerminusDetonator->self, no cast, range 6 circle
    Petrifaction = 26999, // Boss->self, 4.0s cast, range 60 circle
    TentacleWhipRFirst = 27001, // Boss->self, 5.0s cast, range 60 180-degree cone
    TentacleWhipLSecond = 27003, // Helper->self, 7.0s cast, range 60 180-degree cone
    Whack = 27007, // Boss->35F5, 5.0s cast, single-target
}

public enum IconID : uint
{
    Stack = 218, // 35F5
}

public enum TetherID : uint
{
    BombTether = 17, // 35E5->35F5
}

class DeadlyCharge(BossModule module) : Components.ChargeAOEs(module, AID.DeadlyCharge, 5);
class GriefOfParting(BossModule module) : Components.RaidwideCast(module, AID.GriefOfParting);
class DeadlyTentacles(BossModule module) : Components.SingleTargetCast(module, AID.DeadlyTentacles);
class TentacleWhipR1(BossModule module) : Components.StandardAOEs(module, AID.TentacleWhipRFirst, new AOEShapeCone(60, 90.Degrees()));
class TentacleWhipR2(BossModule module) : Components.StandardAOEs(module, AID.TentacleWhipLSecond, new AOEShapeCone(60, 90.Degrees()))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.FindComponent<TentacleWhipR1>()?.Casters.Count > 0)
            yield break;
        else
            foreach (var h in base.ActiveAOEs(slot, actor))
                yield return h;
    }
}
class TentacleWhipL1(BossModule module) : Components.StandardAOEs(module, AID.TentacleWhipLFirst, new AOEShapeCone(60, 90.Degrees()));
class TentacleWhipL2(BossModule module) : Components.StandardAOEs(module, AID.TentacleWhipRSecond, new AOEShapeCone(60, 90.Degrees()))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.FindComponent<TentacleWhipL1>()?.Casters.Count > 0)
            yield break;
        else
            foreach (var h in base.ActiveAOEs(slot, actor))
                yield return h;
    }
}
class SelfDestruct(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly List<Actor> Bombs = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BombTether)
        {
            Bombs.Add(source);
            if (Stacks.Count == 0)
                Stacks.Add(new Stack(WorldState.Actors.Find(tether.Target)!, 3, activation: WorldState.FutureTime(9.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SelfDestruct)
        {
            Bombs.Remove(caster);
            if (Bombs.Count == 0)
                Stacks.Clear();
        }
    }
}
class Petrifaction(BossModule module) : Components.CastGaze(module, AID.Petrifaction);
class Whack(BossModule module) : Components.SingleTargetCast(module, AID.Whack);

class AutoAlphi(BossModule module) : QuestBattle.RotationModule<AlphinaudAI>(module);

class TerminusIdolizerStates : StateMachineBuilder
{
    public TerminusIdolizerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeadlyCharge>()
            .ActivateOnEnter<GriefOfParting>()
            .ActivateOnEnter<TentacleWhipL1>()
            .ActivateOnEnter<TentacleWhipL2>()
            .ActivateOnEnter<TentacleWhipR1>()
            .ActivateOnEnter<TentacleWhipR2>()
            .ActivateOnEnter<DeadlyTentacles>()
            .ActivateOnEnter<SelfDestruct>()
            .ActivateOnEnter<Petrifaction>()
            .ActivateOnEnter<Whack>()
            .ActivateOnEnter<AutoAlphi>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 804, NameID = 10932)]
public class TerminusIdolizer(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300.75f, 151.5f), new ArenaBoundsCircle(19.5f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;
}
