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

class GrimFate(BossModule module) : Components.Cleave(module, AID.GrimFate, new AOEShapeCone(12.6f, 60.Degrees())); // TODO: verify angle
class Desolation(BossModule module) : Components.StandardAOEs(module, AID.Desolation, new AOEShapeRect(60, 3));
class AetherialSurge(BossModule module) : Components.StandardAOEs(module, AID.AetherialSurge, new AOEShapeCircle(6));

// note: actor 'dies' immediately after casting
class SeaOfPitch(BossModule module) : Components.GenericAOEs(module, AID.SeaOfPitch)
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
public class D113Batraal(WorldState ws, Actor primary) : BossModule(ws, primary, new(85, -180), new ArenaBoundsSquare(25))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
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
