namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D123Octomammoth;

public enum OID : uint
{
    Boss = 0x3EAA, // R=26.0
    MammothTentacle = 0x3EAB, // R=6.0
    Crystals = 0x3EAC, // R=0.5
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 33357, // Boss->player, no cast, single-target
    Breathstroke = 34551, // Boss->self, 16.5s cast, range 35 180-degree cone
    Clearout = 33348, // MammothTentacle->self, 9.0s cast, range 16 120-degree cone
    Octostroke = 33347, // Boss->self, 16.0s cast, single-target
    SalineSpit1 = 33352, // Boss->self, 3.0s cast, single-target
    SalineSpit2 = 33353, // Helper->self, 6.0s cast, range 8 circle
    Telekinesis1 = 33349, // Boss->self, 5.0s cast, single-target
    Telekinesis2 = 33351, // Helper->self, 10.0s cast, range 12 circle
    TidalBreath = 33354, // Boss->self, 10.0s cast, range 35 180-degree cone
    TidalRoar = 33356, // Boss->self, 5.0s cast, range 60 circle
    VividEyes = 33355, // Boss->self, 4.0s cast, range 20-26 donut
    WaterDrop = 34436, // Helper->player, 5.0s cast, range 6 circle
    WallopVisual = 33350, // Boss->self, no cast, single-target, visual, starts tentacle wallops
    Wallop = 33346, // MammothTentacle->self, 3.0s cast, range 22 width 8 rect
}

class Wallop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Wallop), new AOEShapeRect(22, 4));
class VividEyes(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VividEyes), new AOEShapeDonut(20, 26));
class Clearout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Clearout), new AOEShapeCone(16, 60.Degrees()));
class TidalBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TidalBreath), new AOEShapeCone(35, 90.Degrees()));
class Breathstroke(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Breathstroke), new AOEShapeCone(35, 90.Degrees()));
class TidalRoar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TidalRoar));
class WaterDrop(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.WaterDrop), 6);
class SalineSpit(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SalineSpit2), new AOEShapeCircle(8));
class Telekinesis(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Telekinesis2), new AOEShapeCircle(12));

class D123OctomammothStates : StateMachineBuilder
{
    public D123OctomammothStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<Clearout>()
            .ActivateOnEnter<VividEyes>()
            .ActivateOnEnter<WaterDrop>()
            .ActivateOnEnter<TidalRoar>()
            .ActivateOnEnter<TidalBreath>()
            .ActivateOnEnter<Telekinesis>()
            .ActivateOnEnter<Breathstroke>()
            .ActivateOnEnter<SalineSpit>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12334)]
class D123Octomammoth(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsUnion([new ArenaBoundsRect(new(-347.74f, -358.78f), 2, 10, -22.5f.Degrees()),
    new ArenaBoundsRect(new(-360.78f, -345.74f), 2, 10, -67.5f.Degrees()), new ArenaBoundsRect(new(-392.26f, -358.78f), 2, 10, 22.5f.Degrees()),
    new ArenaBoundsRect(new(-379.22f, -345.74f), 2, 10, 67.5f.Degrees()), new ArenaBoundsCircle(new(-345, -368), 8), new ArenaBoundsCircle(new(-387.678f, -350.322f), 8),
    new ArenaBoundsCircle(new(-352.322f, -350.322f), 8), new ArenaBoundsCircle(new(-370, -343), 8), new ArenaBoundsCircle(new(-395, -368), 8)]));

