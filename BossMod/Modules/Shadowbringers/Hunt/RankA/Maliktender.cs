namespace BossMod.Shadowbringers.Hunt.RankA.Maliktender;

public enum OID : uint
{
    Boss = 0x2874, // R=3.06
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Sabotendance = 18019, // Boss->self, 3.5s cast, range 8 circle, stuns players
    TwentyKNeedles = 18022, // Boss->self, 3.5s cast, range 20 width 8 rect
    NineNineNineKNeedles = 18024, // Boss->self, 3.0s cast, range 20 width 8 rect
    Haste = 18020, // Boss->self, 3.0s cast, buff to self, boss will use 990k needles instead of 20k needles
}

public enum SID : uint
{
    Haste = 1962,
    Stun = 149,
}

class Sabotendance(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Sabotendance), new AOEShapeCircle(8));
class TwentyKNeedles(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TwentyKNeedles), new AOEShapeRect(20, 4));

class Haste(BossModule module) : BossComponent(module)
{
    private bool HasteB;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Haste)
            HasteB = true;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (HasteB)
            hints.Add("Getting hit by the needle attack will instantly kill you from now on!");
    }
}

class NineNineNineKNeedles(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NineNineNineKNeedles), new AOEShapeRect(20, 4));

class MaliktenderStates : StateMachineBuilder
{
    public MaliktenderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Sabotendance>()
            .ActivateOnEnter<TwentyKNeedles>()
            .ActivateOnEnter<NineNineNineKNeedles>()
            .ActivateOnEnter<Haste>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8901)]
public class Maliktender(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
