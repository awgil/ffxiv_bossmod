namespace BossMod.Stormblood.Quest.OurUnsungHeroes;

public enum OID : uint
{
    Boss = 0x1CAF, // R2.700, x1
    FallenKuribu = 0x18D6, // R0.500, x5
    ShadowSprite = 0x1CB4, // R0.800, x0 (spawn during fight)
}

public enum AID : uint
{
    Glory = 5604, // Boss->self, 3.0s cast, range 40+R 90-degree cone
    CureIV = 8635, // Boss->self, 5.0s cast, range 40 circle
    CureIII1 = 8636, // FallenKuribu->players/1CAD/1CAE, no cast, range 10 circle
    CureV1 = 8637, // FallenKuribu->players, no cast, range 6 circle
    DarkII = 4366, // ShadowSprite->self, 2.5s cast, range 50+R 60-degree cone
}

public enum IconID : uint
{
    CureIII = 71, // player/1CAD/1CAE
    Stack = 62, // player
}

public enum SID : uint
{
    Invincibility = 325, // Boss->Boss, extra=0x0
}

class CureIV(BossModule module) : Components.StandardAOEs(module, AID.CureIV, new AOEShapeCircle(12));
class Glory(BossModule module) : Components.StandardAOEs(module, AID.Glory, new AOEShapeCone(42.7f, 45.Degrees()));
class CureIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.CureIII, AID.CureIII1, 10, 5.15f);
class CureV(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.CureV1, 6, 5.15f);
class DarkII(BossModule module) : Components.StandardAOEs(module, AID.DarkII, new AOEShapeCone(50.8f, 30.Degrees()));

class FallenKuribuStates : StateMachineBuilder
{
    public FallenKuribuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Glory>()
            .ActivateOnEnter<CureIII>()
            .ActivateOnEnter<CureV>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<CureIV>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 265, NameID = 6345)]
public class FallenKuribu(WorldState ws, Actor primary) : BossModule(ws, primary, new(232.3f, 407.7f), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.FindStatus(SID.Invincibility) == null ? 1 : 0;
    }
}
