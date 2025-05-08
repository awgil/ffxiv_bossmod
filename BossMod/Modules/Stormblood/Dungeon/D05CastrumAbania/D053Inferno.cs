namespace BossMod.Stormblood.Dungeon.D05CastrumAbania.D053Inferno;

public enum OID : uint
{
    Boss = 0x1AAE, // R4.500, x?
    InfernoHelper = 0x18D6, //
    LegionPacker = 0x1AAF, // R2.160, x?
    LegionDeathClaw = 0x1AB0, // R1.000, x?
}

public enum AID : uint
{
    Attack = 870, // 1BBB/1BB9/1BBA/1AAA/1BC4/1CF6/1AAE->player, no cast, single-target
    KetuSlash = 7974, // 1AAE->player, 2.0s cast, single-target
    KetuSlash2 = 8331, // 1AAE->player, 2.0s cast, single-target
    KetuCut = 8326, // 1AAE->self, no cast, single-target
    KetuCutter = 7975, // 18D6->self, 4.0s cast, range 20+R 20-degree cone
    KetuRahu = 7973, // 1AAE->self, 4.0s cast, single-target
    KetuWave = 7976, // 18D6->location, 4.0s cast, range 10 circle

    RahuBlaster = 7977, // 1AAE->location, 3.0s cast, range 40+R width 6 rect
    RahuBlaster2 = 8334, // 1AAE->location, 2.0s cast, range 40+R width 6 rect
    RahuRay = 7978, // 18D6->player, no cast, range 10 circle
    RahuCut = 8327, // 1AAE->self, no cast, single-target
    RahuComet = 7979, // 18D6->location, 3.5s cast, range 40 circle
    RahuComet2 = 8328, // 18D6->location, 3.5s cast, range 40 circle
}
public enum IconID : uint
{
    Ray = 74,
}
public enum TetherID : uint
{
    Deathclaw = 1,
}
class KetuSlash(BossModule module) : Components.SingleTargetCast(module, AID.KetuSlash);
class KetuSlash2(BossModule module) : Components.SingleTargetCast(module, AID.KetuSlash2);
class KetuCutter(BossModule module) : Components.StandardAOEs(module, AID.KetuCutter, new AOEShapeCone(20, 10.Degrees()));
class KetuWave(BossModule module) : Components.StandardAOEs(module, AID.KetuWave, 10);
class KetuRahu(BossModule module) : Components.StandardAOEs(module, AID.KetuRahu, new AOEShapeCone(20, 10.Degrees()));
class RahuBlaster(BossModule module) : Components.StandardAOEs(module, AID.RahuBlaster, new AOEShapeRect(44.5f, 3));
class RahuBlaster2(BossModule module) : Components.StandardAOEs(module, AID.RahuBlaster2, new AOEShapeRect(44.5f, 3));
class RahuRay(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(10), (uint)IconID.Ray, AID.RahuRay, centerAtTarget: true);
class RahuComet(BossModule module) : Components.StandardAOEs(module, AID.RahuComet, 20);
class RahuComet2(BossModule module) : Components.StandardAOEs(module, AID.RahuComet2, 20);
class MultiAdds(BossModule module) : Components.AddsMulti(module, [OID.LegionDeathClaw, OID.LegionPacker])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.LegionDeathClaw => 3,
                OID.LegionPacker => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
}
class D053InfernoStates : StateMachineBuilder
{
    public D053InfernoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<KetuSlash>()
            .ActivateOnEnter<KetuSlash2>()
            .ActivateOnEnter<KetuCutter>()
            .ActivateOnEnter<KetuWave>()
            .ActivateOnEnter<RahuBlaster>()
            .ActivateOnEnter<RahuBlaster2>()
            .ActivateOnEnter<RahuRay>()
            .ActivateOnEnter<RahuComet>()
            .ActivateOnEnter<RahuComet2>()
            .ActivateOnEnter<MultiAdds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 242, NameID = 6268)]
public class D053Inferno(WorldState ws, Actor primary) : BossModule(ws, primary, new(282.5f, -27.4f), new ArenaBoundsCircle(19));
