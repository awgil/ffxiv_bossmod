namespace BossMod.Stormblood.Hunt.RankA.Angada;

public enum OID : uint
{
    Boss = 0x1AC0, // R=5.4
}

public enum AID : uint
{
    AutoAttack = 872, // 1AC0->player, no cast, single-target
    ScytheTail = 8190, // 1AC0->self, 3.0s cast, range 4+R circle, knockback 10, away from source + stun
    RockThrow = 8193, // 1AC0->location, 3.0s cast, range 6 circle
    Butcher = 8191, // 1AC0->self, 3.0s cast, range 6+R 120-degree cone
    Rip = 8192, // 1AC0->self, no cast, range 6+R 120-degree cone, always happens directly after Butcher
}

class ScytheTail(BossModule module) : Components.StandardAOEs(module, AID.ScytheTail, new AOEShapeCircle(9.4f));
class Butcher(BossModule module) : Components.StandardAOEs(module, AID.Butcher, new AOEShapeCone(11.4f, 60.Degrees()));

class Rip(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private static readonly AOEShapeCone cone = new(11.4f, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(cone, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Butcher) // boss can move after cast started, so we can't use aoe instance, since that would cause outdated position data to be used
            _activation = Module.CastFinishAt(spell, 1.1f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Rip)
            _activation = default;
    }
}

class RockThrow(BossModule module) : Components.StandardAOEs(module, AID.RockThrow, 6);

class AngadaStates : StateMachineBuilder
{
    public AngadaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScytheTail>()
            .ActivateOnEnter<Butcher>()
            .ActivateOnEnter<Rip>()
            .ActivateOnEnter<RockThrow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 5999)]
public class Angada(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
