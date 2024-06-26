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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12334)]

public class D123Octomammoth(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly Angle Angle225 = 22.5f.Degrees();
    private static readonly Angle Angle675 = 67.5f.Degrees();
    private static readonly Angle Angle0 = 0.Degrees();
    private static readonly Angle Angle45 = 45.Degrees();
    private const float CircleRadius = 7.5f;
    private const int BridgeHalfWidth = 2;
    private const int BridgeLength = 10;
    private const int FillerLength = 2;
    private const float FillerHalfWidth = 0.5f;
    private static readonly List<Shape> Circles = [
            new Circle(new(-345, -368), CircleRadius),
            new Circle(new(-387.678f, -350.322f), CircleRadius),
            new Circle(new(-352.322f, -350.322f), CircleRadius),
            new Circle(new(-370, -343), CircleRadius),
            new Circle(new(-395, -368), CircleRadius)];
    private static readonly List<Shape> Bridges = [
            new Rectangle(new(-347.71f, -359.78f), BridgeHalfWidth, BridgeLength, Angle225),
            new Rectangle(new(-360.77f, -346.3f), BridgeHalfWidth, BridgeLength, Angle675),
            new Rectangle(new(-392.29f, -359.78f), BridgeHalfWidth, BridgeLength, -Angle225),
            new Rectangle(new(-379.22f, -346f), BridgeHalfWidth, BridgeLength, -Angle675)];
    private static readonly List<Shape> Fillers = [
            new Rectangle(new(-390.8f, -361.7f), FillerLength, FillerHalfWidth, -Angle675),
            new Rectangle(new(-394.5f, -360.4f), FillerLength, FillerHalfWidth, Angle225),
            new Rectangle(new(-392.8f, -355.9f), FillerLength, FillerHalfWidth, -Angle675),
            new Rectangle(new(-389.2f, -357.7f), FillerLength, FillerHalfWidth, Angle225),
            new Rectangle(new(-382.3f, -345.1f), FillerLength, FillerHalfWidth, -Angle225),
            new Rectangle(new(-380.2f, -348.6f), FillerLength, FillerHalfWidth, Angle675),
            new Rectangle(new(-376.3f, -347.2f), FillerLength, FillerHalfWidth, -Angle225),
            new Rectangle(new(-377.5f, -343.1f), FillerLength, FillerHalfWidth, Angle675),
            new Rectangle(new(-363.9f, -347.3f), FillerLength, FillerHalfWidth, Angle225),
            new Rectangle(new(-361.9f, -343.9f), FillerLength, FillerHalfWidth, -Angle45),
            new Rectangle(new(-357.9f, -345.1f), FillerLength, FillerHalfWidth, Angle0),
            new Rectangle(new(-359.7f, -348.9f), FillerLength, FillerHalfWidth, -Angle45),
            new Rectangle(new(-351, -357.7f), FillerLength, FillerHalfWidth, -Angle45),
            new Rectangle(new(-347.3f, -356.2f), FillerLength, FillerHalfWidth, Angle675),
            new Rectangle(new(-345.4f, -360.4f), FillerLength, FillerHalfWidth, -Angle225),
            new Rectangle(new(-349.1f, -361.8f), FillerLength, FillerHalfWidth, Angle675)];
    private static readonly ArenaBoundsComplex arena = new(Bridges.Concat(Circles).Concat(Fillers));
}
