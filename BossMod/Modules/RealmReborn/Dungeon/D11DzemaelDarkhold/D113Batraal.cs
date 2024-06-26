namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D113Batraal;

public enum OID : uint
{
    Boss = 0x60A, // x1
    CorruptedCrystal = 0x60C, // spawn during fight
    VoidPitch = 0x6B4, // spawn during fight
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    GrimCleaver = 620, // Boss->player, no cast, single-target, random
    GrimFate = 624, // Boss->self, no cast, range 8+4.6 ?-degree cone cleave
    Desolation = 958, // Boss->self, 2.3s cast, range 55+4.6 width 6 rect aoe
    Hellssend = 1132, // Boss->self, no cast, damage up buff
    AetherialSurge = 1167, // CorruptedCrystal->self, 3.0s cast, range 5+1 circle aoe
    SeaOfPitch = 962, // VoidPitch->location, no cast, range 4 circle
}

class GrimFate(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.GrimFate), new AOEShapeCone(12.6f, 60.Degrees())); // TODO: verify angle
class Desolation(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Desolation), new AOEShapeRect(60, 3));
class AetherialSurge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherialSurge), new AOEShapeCircle(6));

// note: actor 'dies' immediately after casting
class SeaOfPitch(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.SeaOfPitch))
{
    private readonly AOEShape _shape = new AOEShapeCircle(4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: proper timings...
        return Module.Enemies(OID.VoidPitch).Where(a => !a.IsDead).Select(a => new AOEInstance(_shape, a.Position));
    }
}

class D113BatraalStates : StateMachineBuilder
{
    public D113BatraalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GrimFate>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<AetherialSurge>()
            .ActivateOnEnter<SeaOfPitch>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 13, NameID = 1396)]
public class D113Batraal(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> shape = [new PolygonCustom([new(69.7f, -150.8f), new(84.3f, -160.6f), new(100.8f, -150.9f),
    new(103.5f, -156.9f), new(102.8f, -163), new(94.3f, -181.3f), new(84.6f, -178.8f),
    new(82.2f, -185.4f), new(81.7f, -202.9f), new(76.8f, -202.5f), new(72.2f, -203.4f), new(58.4f, -198.1f),
    new(57.6f, -193.1f), new(65.2f, -188.8f), new(73.1f, -183.7f), new(75.1f, -178.7f), new(70.9f, -173.1f),
    new(59.4f, -164.6f), new(60.8f, -159), new(58.4f, -158.1f), new(56f, -154.1f)])];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(shape);

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.CorruptedCrystal => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
