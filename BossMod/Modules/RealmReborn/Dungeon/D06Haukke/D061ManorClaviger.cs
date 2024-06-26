namespace BossMod.RealmReborn.Dungeon.D06Haukke.D061ManorClaviger;

public enum OID : uint
{
    Boss = 0x110, // x1
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast
    SweetSteel = 489, // Boss->self, no cast, range 7.4 ?-degree cone cleave
    VoidFire2 = 855, // Boss->location, 3.0s cast, range 5 aoe
    DarkMist = 705, // Boss->self, 4.0s cast, range 9.4 aoe
}

class SweetSteel(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.SweetSteel), new AOEShapeCone(7.4f, 45.Degrees())); // TODO: verify angle
class VoidFire2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.VoidFire2), 5);
class DarkMist(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkMist), new AOEShapeCircle(9.4f));

class D061ManorClavigerStates : StateMachineBuilder
{
    public D061ManorClavigerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SweetSteel>()
            .ActivateOnEnter<VoidFire2>()
            .ActivateOnEnter<DarkMist>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 6, NameID = 423)]
public class D061ManorClaviger(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> shape = [new PolygonCustom([new(-21.5f, 3), new(-14, 9.2f), new(-14.1f, 14),
    new(8.2f, 14.7f), new(16.5f, 12.3f), new(16.5f, 7), new(22.1f, 7),
    new(21.5f, -9.4f), new(12.6f, -14.5f), new(-15.9f, -13.1f), new(-16.1f, -10), new(-20.3f, -10)])];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(shape);
}
