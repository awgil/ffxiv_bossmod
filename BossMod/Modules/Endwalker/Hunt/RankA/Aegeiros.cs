namespace BossMod.Endwalker.Hunt.RankA.Aegeiros;

public enum OID : uint
{
    Boss = 0x3671, // R7.500, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Leafstorm = 27708, // Boss->self, 6.0s cast, range 10 circle
    Rimestorm = 27709, // Boss->self, 1.0s cast, range 40 180-degree cone
    Snowball = 27710, // Boss->location, 3.0s cast, range 8 circle
    Canopy = 27711, // Boss->players, no cast, range 12 120-degree cone cleave
    BackhandBlow = 27712, // Boss->self, 3.0s cast, range 12 120-degree cone
}

class LeafstormRimestorm(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _rimestormExpected;
    private static readonly AOEShapeCircle _leafstorm = new(10);
    private static readonly AOEShapeCone _rimestorm = new(40, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.Leafstorm) ?? false)
            yield return new(_leafstorm, Module.PrimaryActor.Position, Module.PrimaryActor.CastInfo!.Rotation, Module.PrimaryActor.CastInfo.NPCFinishAt);

        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.Rimestorm) ?? false)
            yield return new(_rimestorm, Module.PrimaryActor.Position, Module.PrimaryActor.CastInfo!.Rotation, Module.PrimaryActor.CastInfo.NPCFinishAt);
        else if (_rimestormExpected != default)
            yield return new(_rimestorm, Module.PrimaryActor.Position, Module.PrimaryActor.CastInfo?.Rotation ?? Module.PrimaryActor.Rotation, _rimestormExpected);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (caster == Module.PrimaryActor && (AID)spell.Action.ID == AID.Leafstorm)
            _rimestormExpected = WorldState.FutureTime(9.6f);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster == Module.PrimaryActor && (AID)spell.Action.ID == AID.Rimestorm)
            _rimestormExpected = new();
    }
}

class Snowball(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Snowball), 8);
class Canopy(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Canopy), new AOEShapeCone(12, 60.Degrees()), activeWhileCasting: false);
class BackhandBlow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BackhandBlow), new AOEShapeCone(12, 60.Degrees()));

class AegeirosStates : StateMachineBuilder
{
    public AegeirosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LeafstormRimestorm>()
            .ActivateOnEnter<Snowball>()
            .ActivateOnEnter<Canopy>()
            .ActivateOnEnter<BackhandBlow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 10628)]
public class Aegeiros(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
