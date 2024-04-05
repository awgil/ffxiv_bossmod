namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D113Batraal;

public enum OID : uint
{
    Boss = 0x60A, // x1
    CorruptedCrystal = 0x60C, // spawn during fight
    VoidPitch = 0x6B4, // spawn during fight
};

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    GrimCleaver = 620, // Boss->player, no cast, single-target, random
    GrimFate = 624, // Boss->self, no cast, range 8+4.6 ?-degree cone cleave
    Desolation = 958, // Boss->self, 2.3s cast, range 55+4.6 width 6 rect aoe
    Hellssend = 1132, // Boss->self, no cast, damage up buff
    AetherialSurge = 1167, // CorruptedCrystal->self, 3.0s cast, range 5+1 circle aoe
    SeaOfPitch = 962, // VoidPitch->location, no cast, range 4 circle
};

class GrimFate : Components.Cleave
{
    public GrimFate() : base(ActionID.MakeSpell(AID.GrimFate), new AOEShapeCone(12.6f, 60.Degrees())) { } // TODO: verify angle
}

class Desolation : Components.SelfTargetedAOEs
{
    public Desolation() : base(ActionID.MakeSpell(AID.Desolation), new AOEShapeRect(60, 3)) { }
}

class AetherialSurge : Components.SelfTargetedAOEs
{
    public AetherialSurge() : base(ActionID.MakeSpell(AID.AetherialSurge), new AOEShapeCircle(6)) { }
}

// note: actor 'dies' immediately after casting
class SeaOfPitch : Components.GenericAOEs
{
    private AOEShape _shape = new AOEShapeCircle(4);

    public SeaOfPitch() : base(ActionID.MakeSpell(AID.SeaOfPitch)) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        // TODO: proper timings...
        return module.Enemies(OID.VoidPitch).Where(a => !a.IsDead).Select(a => new AOEInstance(_shape, a.Position));
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
public class D113Batraal : BossModule
{
    public D113Batraal(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(85, -180), 25)) { }

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
