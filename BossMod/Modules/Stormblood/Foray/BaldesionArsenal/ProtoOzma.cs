namespace BossMod.Stormblood.Foray.BaldesionArsenal.ProtoOzma;

public enum OID : uint
{
    Boss = 0x25E8,
    Helper = 0x233C,
}

public enum AID : uint
{
    //_Weaponskill_Attack = 14251, // 2629->player, no cast, single-target
    //_Weaponskill_Transfiguration = 14258, // Boss->self, no cast, single-target
    //_Weaponskill_MourningStar = 14260, // Boss->self, no cast, single-target
    //_Weaponskill_MourningStar1 = 14261, // 2629->self, no cast, range 27 circle
    //_Weaponskill_ShootingStar = 14263, // Boss->self, 5.0s cast, single-target
    //_Weaponskill_ShootingStar1 = 14264, // 2629->self, 5.0s cast, range 26 circle
    //_Weaponskill_Transfiguration1 = 14259, // Boss->self, no cast, single-target
    //_Weaponskill_BlackHole = 14237, // Boss->self, no cast, range 40 circle
    //_Weaponskill_Transfiguration2 = 14238, // 25E9/Boss->self, no cast, single-target
    //_Weaponskill_FlareStar = 14240, // 25E9/Boss->self, no cast, single-target
    //_Weaponskill_FlareStar1 = 14241, // 2629->self, no cast, range 38+R circle
    //_Weaponskill_Transfiguration3 = 14244, // Boss/25E9->self, no cast, single-target
    //_Weaponskill_Execration = 14246, // Boss/25E9->self, no cast, single-target
    //_Weaponskill_Execration1 = 14247, // 2629->self, no cast, range 40+R width 11 rect
    //_Weaponskill_AccelerationBomb = 14250, // Boss->self, no cast, ???
    //_Weaponskill_MeteorImpact = 14256, // 25EB->self, 4.0s cast, range 20 circle
    //_AutoAttack_Attack = 872, // 25EB->player, no cast, single-target
    //_Weaponskill_Meteor = 14248, // 2629->location, no cast, range 10 circle
    //_Weaponskill_Transfiguration4 = 14245, // Boss->self, no cast, single-target
    //_Weaponskill_Explosion = 14242, // 25EA->self, no cast, range 6 circle
    //_Weaponskill_Holy = 14249, // Boss->self, 4.0s cast, range 50 circle
}

class ProtoOzmaStates : StateMachineBuilder
{
    public ProtoOzmaStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 639, NameID = 7981)]
public class ProtoOzma(WorldState ws, Actor primary) : BossModule(ws, primary, new(-17, 29), OzmaBounds)
{
    public static readonly ArenaBoundsCustom OzmaBounds = MakeBounds();

    private static ArenaBoundsCustom MakeBounds()
    {
        var approx = CurveApprox.Donut(18.75f, 25f, 1 / 90f);
        var rect1 = CurveApprox.Rect(new(5, 0), new(0, 12)).Select(off => new WDir(0, 24.5f) + off);
        var clipper = new PolygonClipper();
        var u1 = clipper.Union(new(rect1), new(approx));
        var u2 = clipper.Union(new(rect1.Select(d => d.Rotate(120.Degrees()))), new(u1));
        var u3 = clipper.Union(new(rect1.Select(d => d.Rotate(240.Degrees()))), new(u2));
        return new(36.5f, u3);
    }

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        Arena.ActorInsideBounds(Center, PrimaryActor.Rotation, ArenaColor.Enemy);
    }
}

