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

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "dhoggpt, Malediktus, xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12334)]
class D123Octomammoth(WorldState ws, Actor primary) : BossModule(ws, primary, new(-370, -355.5f), OctoBounds)
{
    public static readonly ArenaBoundsCustom OctoBounds = MakeBounds();

    private static ArenaBoundsCustom MakeBounds()
    {
        var island = CurveApprox.Circle(8, 1 / 90f);
        var iop = new PolygonClipper.Operand();
        var rot = -90.Degrees();

        for (var i = 0; i < 5; i++)
        {
            var off = rot.ToDirection() * 25;
            var thisIsland = island.Select(d => d + off);
            iop.AddContour(island.Select(d => d + off));

            rot += 45.Degrees();
        }

        var bop = new PolygonClipper.Operand();
        rot = -67.5f.Degrees();

        List<WDir> bridgeContour = [
            new(-2.745f, 21.225f),
            new(0, 21.725f),
            new(2.745f, 21.225f),
            new(2.745f, 26.467f),
            new(0, 25.775f),
            new(-2.745f, 26.467f),
        ];

        for (var i = 0; i < 4; i++)
        {
            bop.AddContour(bridgeContour.Select(b => b.Rotate(rot)));
            rot += 45.Degrees();
        }

        return new(33, new PolygonClipper().Union(iop, bop).Transform(new(0, -12.5f), new(0, 1)));
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.ActorInsideBounds(PrimaryActor.Position, PrimaryActor.Rotation, ArenaColor.Enemy);
}
