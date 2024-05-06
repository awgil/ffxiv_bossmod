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

class WaterIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.WaterIII), 8);
class PelagicCleaver1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PelagicCleaver1), new AOEShapeCone(40, 30.Degrees())); // note: it's interruptible, but that's not worth the hint
class PelagicCleaver2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PelagicCleaver2), new AOEShapeCone(40, 30.Degrees())); // note: it's interruptible, but that's not worth the hint
class WaterFlood(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WaterFlood), new AOEShapeCircle(6));
class DivineFlood(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DivineFlood), new AOEShapeCircle(6));

public class A30Trash1States : StateMachineBuilder
{
    public A30Trash1States(A30Trash1 module) : base(module)
    {
        // as soon as the last serpent dies, other adds are spawned; serpents are destroyed a bit later
        TrivialPhase()
            .ActivateOnEnter<WaterIII>()
            .Raw.Update = () => module.Enemies(OID.Serpent).All(e => e.IsDead);
        TrivialPhase(1)
            .ActivateOnEnter<PelagicCleaver1>()
            .ActivateOnEnter<PelagicCleaver2>()
            .ActivateOnEnter<WaterFlood>()
            .ActivateOnEnter<DivineFlood>()
            .Raw.Update = () => module.Enemies(OID.Serpent).Count == 0 && module.Enemies(OID.Triton).All(e => e.IsDead) && module.Enemies(OID.DivineSprite).All(e => e.IsDead) && module.Enemies(OID.WaterSprite).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", PrimaryActorOID = (uint)OID.Serpent, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12478, SortOrder = 1)]
public class A30Trash1(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, -800), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Serpent), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Triton), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.DivineSprite), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.WaterSprite), ArenaColor.Enemy);
    }
}
