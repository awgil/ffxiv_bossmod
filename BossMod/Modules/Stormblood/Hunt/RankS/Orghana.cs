namespace BossMod.Stormblood.Hunt.RankS.Orghana;

public enum OID : uint
{
    Boss = 0x1AB3, // R=5.04
};

public enum AID : uint
{
    AutoAttack = 7998, // Boss->player, no cast, single-target
    FlamingEpigraph = 7999, // Boss->location, no cast, range 10 circle, targets random player
    TremblingEpigraph = 8000, // Boss->self, 5,0s cast, range 40 circle, usually raidwide x4
    TremblingEpigraph2 = 8001, // Boss->self, no cast, range 40 circle
    FlaringEpigraph = 8002, // Boss->self, 5,0s cast, range 40 circle
    Epigraph = 7997, // Boss->self, 3,5s cast, range 50+R width 8 rect
};

class TremblingEpigraph : Components.RaidwideCast
{
    public TremblingEpigraph() : base(ActionID.MakeSpell(AID.TremblingEpigraph), "Raidwide x4") { }
}

class FlaringEpigraph : Components.RaidwideCast
{
    public FlaringEpigraph() : base(ActionID.MakeSpell(AID.FlaringEpigraph)) { }
}

class Epigraph : Components.SelfTargetedAOEs
{
    public Epigraph() : base(ActionID.MakeSpell(AID.Epigraph), new AOEShapeRect(55.04f, 4)) { }
}

class OrghanaStates : StateMachineBuilder
{
    public OrghanaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TremblingEpigraph>()
            .ActivateOnEnter<FlaringEpigraph>()
            .ActivateOnEnter<Epigraph>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 5986)]
public class Orghana : SimpleBossModule
{
    public Orghana(WorldState ws, Actor primary) : base(ws, primary) { }
}
