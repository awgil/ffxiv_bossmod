namespace BossMod.Global.Quest.FF15Collab.MAx;

public enum OID : uint
{
    Boss = 0x252B, //R=6.75
    MagitekTrooper = 0x252A, //R=0.9
    Helper = 0x233C,
    Noctis = 0x2529,
}

public enum AID : uint
{
    AutoAttack = 14675, // 252A->player/Noctis, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target
    Chainsaw = 14601, // Boss->self, 3.0s cast, range 10 90-degree cone
    MagitekMissile = 14598, // Boss->self, 1.0s cast, single-target
    MagitekMissile2 = 14599, // Helper->location, 3.0s cast, range 5 circle
    Shock = 14600, // Boss->self, 3.0s cast, range 10 circle
    unknown = 14531, // Boss->self, no cast, single-target
    unknown2 = 14533, // Boss->self, no cast, single-target
}

class Chainsaw(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Chainsaw), new AOEShapeCone(10, 45.Degrees()));
class Shock(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shock), new AOEShapeCircle(10));
class MagitekMissile(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekMissile2), 5);

class MAxStates : StateMachineBuilder
{
    public MAxStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Chainsaw>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<MagitekMissile>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68694, NameID = 7898)] // also: fate 1409
public class MAx(WorldState ws, Actor primary) : BossModule(ws, primary, new(295, -22), new ArenaBoundsCircle(25))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Noctis))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.MagitekTrooper))
            Arena.Actor(s, ArenaColor.Object);
    }
}
