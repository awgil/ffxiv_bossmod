namespace BossMod.RealmReborn.Dungeon.D26Snowcloak.D263Fenrir;

public enum OID : uint
{
    Boss = 0x3979, // R5.850, x?
    Icicle = 0x397A, // R2.500, x?
    Helper = 0x233C, // R0.500, x?, Helper type
}
public enum AID : uint
{
    Attack = 872, // D20/D1B/D23/D1D/D19/D24/D07/D1F/3977/D30/D1C/D22/3979->player, no cast, single-target
    ThousandYearStorm = 29594, // 3979->self, 5.0s cast, single-target
    ThousandYearStorm1 = 29595, // 233C->self, 5.0s cast, range 100 circle
    HowlingMoon = 29598, // 3979->self, no cast, ???
    EclipticBite = 29596, // 3979->player, 5.0s cast, single-target
    PillarImpact = 29600, // 397A/player->self, 2.5s cast, range 4 circle
    HeavenswardRoar = 29593, // 3979->self, 5.0s cast, range 50 60-degree cone
    PillarShatterEarly = 29648, // 397A/player->self, 6.0s cast, range 8 circle
    PillarShatterLate = 29601, // 397A->self, 2.0s cast, range 8 circle
    LunarCry = 29599, // 3979->self, 8.0s cast, range 80 circle

    A = 29597, // 3979->location, no cast, ???
}
public enum SID : uint
{
    DeepFreeze = 487,
}
public enum IconID : uint
{
    TankLockon = 218,
}
class PillarImpact(BossModule module) : Components.StandardAOEs(module, AID.PillarImpact, new AOEShapeCircle(4));
class PillarShatterEarly(BossModule module) : Components.StandardAOEs(module, AID.PillarShatterEarly, new AOEShapeCircle(8));
class PillarShatterLate(BossModule module) : Components.StandardAOEs(module, AID.PillarShatterLate, new AOEShapeCircle(8));
class LunarCry(BossModule module) : Components.CastLineOfSightAOE(module, AID.LunarCry, 80f, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.Icicle).Where(e => e.ModelState.AnimState1 == 0);
}
class EclipticBite(BossModule module) : Components.SingleTargetCast(module, AID.EclipticBite);
class HeavenswardRoar(BossModule module) : Components.StandardAOEs(module, AID.HeavenswardRoar, new AOEShapeCone(50, 30.Degrees()));
class D263FenrirStates : StateMachineBuilder
{
    public D263FenrirStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PillarImpact>()
            .ActivateOnEnter<PillarShatterEarly>()
            .ActivateOnEnter<PillarShatterLate>()
            .ActivateOnEnter<EclipticBite>()
            .ActivateOnEnter<HeavenswardRoar>()
            .ActivateOnEnter<LunarCry>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 27, NameID = 3044)]
public class D263Fenrir(WorldState ws, Actor primary) : BossModule(ws, primary, new(0.99f, 64.98f), new ArenaBoundsCircle(25));
