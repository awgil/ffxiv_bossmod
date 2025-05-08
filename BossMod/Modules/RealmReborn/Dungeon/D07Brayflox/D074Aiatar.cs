namespace BossMod.RealmReborn.Dungeon.D07Brayflox.D074Aiatar;

public enum OID : uint
{
    Boss = 0x38C5, // x1
    Helper = 0x233C, // x14
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast
    SalivousSnap = 28659, // Boss->player, 5.0s cast, tankbuster
    ToxicVomit = 28656, // Boss->self, 3.0s cast, visual
    ToxicVomitAOE = 28657, // Helper->self, 5.0s cast, range 2 aoe
    Burst = 28658, // Helper->self, 9.0s cast, range 10 aoe
    DragonBreath = 28660, // Boss->self, 3.0s cast, range 30 width 8 rect
}

class SalivousSnap(BossModule module) : Components.SingleTargetCast(module, AID.SalivousSnap);
class ToxicVomit(BossModule module) : Components.StandardAOEs(module, AID.ToxicVomitAOE, new AOEShapeCircle(2));
class Burst(BossModule module) : Components.StandardAOEs(module, AID.Burst, new AOEShapeCircle(10), 4);
class DragonBreath(BossModule module) : Components.StandardAOEs(module, AID.DragonBreath, new AOEShapeRect(30, 4));

class D074AiatarStates : StateMachineBuilder
{
    public D074AiatarStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SalivousSnap>()
            .ActivateOnEnter<ToxicVomit>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<DragonBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 8, NameID = 1279)]
public class D074Aiatar(WorldState ws, Actor primary) : BossModule(ws, primary, new(-25, -235), new ArenaBoundsCircle(20));
