using BossMod.Components;

namespace BossMod.Stormblood.Dungeon.D01SirensongSea.D011Lugat;

public enum OID : uint
{
    Gen = 0x18D6,   // R0.500, x?
    GenActor1ea2f4 = 0x1EA2F4, // R2.000, x?, EventObj type
    GenActor1e8f2f = 0x1E8F2F, // R0.500, x?, EventObj type
    Boss = 0x1AFB,   // R4.500, x?
    GenActor1ea2fd = 0x1EA2FD, // R2.000, x?, EventObj type
    GenActor1e8fb8 = 0x1E8FB8, // R2.000, x?, EventObj type
    GenActor1ea2f2 = 0x1EA2F2, // R2.000, x?, EventObj type
    Gen2 = 0xF9747,  // R0.500, x?, EventNpc type
    GenActor1ea2f1 = 0x1EA2F1, // R2.000, x?, EventObj type
    GenActor1e8536 = 0x1E8536, // R2.000, x?, EventObj type
    GenShortcut = 0x1E873C, // R0.500, x?, EventObj type
}

public enum AID : uint
{
    AmorphousApplause = 8022,
    Hydroball = 8023,
    ConcussiveOscillationBoss = 8026,
    ConcussiveOscillation = 8027
}

public enum IOD : uint
{
    Stack = 62
}
class ConcussiveOscillationBoss(BossModule module) : StandardAOEs(module, AID.ConcussiveOscillationBoss, 7);
class ConcussiveOscillation(BossModule module) : StandardAOEs(module, AID.ConcussiveOscillation, 8);
class AmorphousApplause(BossModule module) : StandardAOEs(module, AID.AmorphousApplause, new AOEShapeCone(30, 90.Degrees()));

class Hydroball(BossModule module) : StackWithCastTargets(module, AID.Hydroball, 5, 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        Actor? actorCheck = WorldState.Party[1];
        if (actorCheck != null)
        {
            foreach (var s in ActiveStacks)
            {
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(actorCheck.Position, 3), s.Activation);
            }
        }
    }
}

class D011LugatStates : StateMachineBuilder
{
    public D011LugatStates(BossModule module) : base(module)
    {
        TrivialPhase().
            ActivateOnEnter<Hydroball>().
            ActivateOnEnter<AmorphousApplause>().
            ActivateOnEnter<ConcussiveOscillation>().
            ActivateOnEnter<ConcussiveOscillationBoss>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 238, NameID = 6071)]
public class D011Lugat(WorldState ws, Actor primary) : BossModule(ws, primary, new WPos(-1.465f, -217.254f), new ArenaBoundsCircle(20));
