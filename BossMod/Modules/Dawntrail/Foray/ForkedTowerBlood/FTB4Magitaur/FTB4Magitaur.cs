namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class UnsealedAura(BossModule module) : Components.RaidwideCast(module, (uint)AID.UnsealedAura);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Magitaur, GroupType = BossModuleInfo.GroupType.TheForkedTowerBlood, GroupID = 1018u, NameID = 13947u, PlanLevel = 100, SortOrder = 5, Category = BossModuleInfo.Category.Foray, Expansion = BossModuleInfo.Expansion.Dawntrail)]
public sealed class FTB4Magitaur : BossModule
{
    public FTB4Magitaur(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private FTB4Magitaur(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena, true) { }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(700f, -674f), 31.5f, 90)], [new Rectangle(new(700f, -705.916f), 7.5f, 1.25f), new Rectangle(new(700f, -641.5f), 7.5f, 1.25f)]);
        return (arena.Center, arena);
    }

    public static WPos[] GetSquarePositions() => [new(700f, -659.504f), new(712.554f, -681.248f), new(687.443f, -681.25f)]; // starting in south, ccw order
    public static Angle[] GetSquareAngles() => [-45f.Degrees(), -15f.Degrees(), 105f.Degrees()];
    public static WDir[] GetSquareAnglesDirs() => [-45f.Degrees().ToDirection(), -15f.Degrees().ToDirection(), 105f.Degrees().ToDirection()];
    public static Square[] GetSquares() => [new Square(new(700f, -659.504f), 10f, -45f.Degrees()), new Square(new(712.554f, -681.248f), 10f, -15f.Degrees()),
    new Square(new(687.443f, -681.25f), 10f, 105f.Degrees())];
    public static AOEShapeCustom GetCircleMinusSquares(WPos center) => new(center, [new Square(new(700f, -674f), 31.5f)], GetSquares());
}
