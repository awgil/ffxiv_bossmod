namespace BossMod.Endwalker.Alliance.A30Trash1;

public enum OID : uint
{
    Serpent = 0x4010, // R3.450, x6
    Triton = 0x4011, // R1.950, x0, x2 spawn during fight
    DivineSprite = 0x4012, // R1.600, x0, x3 spawn during fight
    WaterSprite = 0x4085, // R0.800, x0, x5 spawn during fight
}

public enum AID : uint
{
    AutoAttack = 870, // Serpent/Triton->player, no cast, single-target
    WaterIII = 35438, // Serpent->location, 4.0s cast, range 8 circle
    PelagicCleaver1 = 35439, // Triton->self, 5.0s cast, range 40 60-degree cone
    PelagicCleaver2 = 35852, // Triton->self, 5.0s cast, range 40 60-degree cone
    WaterAutoAttack = 35469, // WaterSprite/DivineSprite->player, no cast, single-target, auto attack
    WaterFlood = 35442, // WaterSprite->self, 3.0s cast, range 6 circle
    WaterBurst = 35443, // WaterSprite->self, no cast, range 40 circle, raidwide when Water Sprite dies
    DivineFlood = 35440, // DivineSprite->self, 3.0s cast, range 6 circle
    DivineBurst = 35441, // DivineSprite->self, no cast, range 40 circle, raidwide when Divine Sprite dies
}

class WaterIII() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.WaterIII), 8);
class PelagicCleaver1() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.PelagicCleaver1), new AOEShapeCone(40, 30.Degrees()));
class PelagicCleaver2() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.PelagicCleaver2), new AOEShapeCone(40, 30.Degrees()));
class PelagicCleaver1InterruptHint() : Components.CastInterruptHint(ActionID.MakeSpell(AID.PelagicCleaver1));
class PelagicCleaver2InterruptHint() : Components.CastInterruptHint(ActionID.MakeSpell(AID.PelagicCleaver2));
class WaterFlood() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.WaterFlood), new AOEShapeCircle(6));
class DivineFlood() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.DivineFlood), new AOEShapeCircle(6));

public class A30Trash1States : StateMachineBuilder
{
    public A30Trash1States(A30Trash1 module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WaterIII>()
            .Raw.Update = () => module.Serpents.All(e => e.IsDeadOrDestroyed);
        TrivialPhase(1)
            .ActivateOnEnter<PelagicCleaver1>()
            .ActivateOnEnter<PelagicCleaver2>()
            .ActivateOnEnter<PelagicCleaver1InterruptHint>()
            .ActivateOnEnter<PelagicCleaver2InterruptHint>()
            .ActivateOnEnter<WaterFlood>()
            .ActivateOnEnter<DivineFlood>()
            .Raw.Update = () => module.Serpents.All(e => e.IsDestroyed) && module.Tritons.All(e => e.IsDeadOrDestroyed) && module.DivineSprites.All(e => e.IsDeadOrDestroyed) && module.WaterSprites.All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", PrimaryActorOID = (uint)OID.Serpent, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12478, SortOrder = 1)]
public class A30Trash1 : BossModule
{
    public readonly IReadOnlyList<Actor> Serpents; // available from start
    public readonly IReadOnlyList<Actor> Tritons; // spawned after all serpents are dead
    public readonly IReadOnlyList<Actor> DivineSprites; // spawned after all serpents are dead
    public readonly IReadOnlyList<Actor> WaterSprites; // spawned after all serpents are dead

    public A30Trash1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-800, -800), 20))
    {
        Serpents = Enemies(OID.Serpent);
        Tritons = Enemies(OID.Triton);
        DivineSprites = Enemies(OID.DivineSprite);
        WaterSprites = Enemies(OID.WaterSprite);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Serpents, ArenaColor.Enemy);
        Arena.Actors(Tritons, ArenaColor.Enemy);
        Arena.Actors(DivineSprites, ArenaColor.Enemy);
        Arena.Actors(WaterSprites, ArenaColor.Enemy);
    }
}
