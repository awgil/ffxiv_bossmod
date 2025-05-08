namespace BossMod.Shadowbringers.Quest.TheOracleOfLight;

public enum OID : uint
{
    Boss = 0x299D,
    Helper = 0x233C,
}

public enum AID : uint
{
    HotPursuit1 = 17622, // 2AF0->location, 3.0s cast, range 5 circle
    NexusOfThunder1 = 17621, // 2AF0->self, 7.0s cast, range 60+R width 5 rect
    NexusOfThunder2 = 17823, // 2AF0->self, 8.5s cast, range 60+R width 5 rect
    Burn = 18035, // 2BE6->self, 4.5s cast, range 8 circle
    UnbridledWrath = 18036, // 299E->self, 5.5s cast, range 90 width 90 rect
}

class HotPursuit(BossModule module) : Components.StandardAOEs(module, AID.HotPursuit1, 5);
class NexusOfThunder1(BossModule module) : Components.StandardAOEs(module, AID.NexusOfThunder1, new AOEShapeRect(60, 2.5f));
class NexusOfThunder2(BossModule module) : Components.StandardAOEs(module, AID.NexusOfThunder2, new AOEShapeRect(60, 2.5f));
class Burn(BossModule module) : Components.StandardAOEs(module, AID.Burn, new AOEShapeCircle(8), maxCasts: 8);
class UnbridledWrath(BossModule module) : Components.KnockbackFromCastTarget(module, AID.UnbridledWrath, 20, kind: Kind.DirForward, stopAtWall: true);

class RanjitStates : StateMachineBuilder
{
    public RanjitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HotPursuit>()
            .ActivateOnEnter<NexusOfThunder1>()
            .ActivateOnEnter<NexusOfThunder2>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<UnbridledWrath>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68841, NameID = 8374)]
public class Ranjit(WorldState ws, Actor primary) : BossModule(ws, primary, new(126.75f, -311.25f), new ArenaBoundsCircle(20));
