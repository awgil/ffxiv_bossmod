namespace BossMod.Shadowbringers.Hunt.RankA.Baal;

public enum OID : uint
{
    Boss = 0x2854, // R=3.2
}

public enum AID : uint
{
    AutoAttack = 872, // 2854/2850->player, no cast, single-target
    SewerWater = 17956, // 2854->self, 3.0s cast, range 12 180-degree cone
    SewerWater2 = 17957, // 2854->self, 3.0s cast, range 12 180-degree cone
    SewageWave = 17423, // 2854->self, 5.0s cast, range 30 180-degree cone
    SewageWave1 = 17422, // 2854->self, no cast, range 30 180-degree cone
    SewageWave2 = 17424, // 2854->self, 5.0s cast, range 30 180-degree cone
    SewageWave3 = 17421, // 2854->self, no cast, range 30 180-degree cone
}

class SewerWater(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SewerWater), new AOEShapeCone(12, 90.Degrees()));
class SewerWater2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SewerWater2), new AOEShapeCone(12, 90.Degrees()));

class SewageWave(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(30, 90.Degrees());
    private DateTime _activation;
    private Angle _rotation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            if (NumCasts == 0)
            {
                yield return new(cone, Module.PrimaryActor.Position, _rotation, _activation, ArenaColor.Danger);
                yield return new(cone, Module.PrimaryActor.Position, _rotation + 180.Degrees(), _activation.AddSeconds(2.3f), Risky: false);
            }
            if (NumCasts == 1)
                yield return new(cone, Module.PrimaryActor.Position, _rotation + 180.Degrees(), _activation.AddSeconds(2.3f), ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SewageWave or AID.SewageWave2)
        {
            _rotation = spell.Rotation;
            _activation = spell.NPCFinishAt;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SewageWave or AID.SewageWave2)
            ++NumCasts;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SewageWave1 or AID.SewageWave3)
        {
            NumCasts = 0;
            _activation = default;
        }
    }
}

class BaalStates : StateMachineBuilder
{
    public BaalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SewageWave>()
            .ActivateOnEnter<SewerWater>()
            .ActivateOnEnter<SewerWater2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8897)]
public class Baal(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
