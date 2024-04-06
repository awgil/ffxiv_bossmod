namespace BossMod.Endwalker.Alliance.A30TrashPack1;

class WaterIII : Components.LocationTargetedAOEs
{
    public WaterIII() : base(ActionID.MakeSpell(AID.WaterIII), 8) { }
}

class PelagicCleaver1 : Components.SelfTargetedAOEs
{
    public PelagicCleaver1() : base(ActionID.MakeSpell(AID.PelagicCleaver1), new AOEShapeCone(40, 30.Degrees())) { }
}

class PelagicCleaver2 : Components.SelfTargetedAOEs
{
    public PelagicCleaver2() : base(ActionID.MakeSpell(AID.PelagicCleaver2), new AOEShapeCone(40, 30.Degrees())) { }
}

class PelagicCleaver1Hint : Components.CastInterruptHint
{
    public PelagicCleaver1Hint() : base(ActionID.MakeSpell(AID.PelagicCleaver1)) { }
}

class PelagicCleaver2Hint : Components.CastInterruptHint
{
    public PelagicCleaver2Hint() : base(ActionID.MakeSpell(AID.PelagicCleaver2)) { }
}

class WaterFlood : Components.SelfTargetedAOEs
{
    public WaterFlood() : base(ActionID.MakeSpell(AID.WaterFlood), new AOEShapeCircle(6)) { }
}

class DivineFlood : Components.SelfTargetedAOEs
{
    public DivineFlood() : base(ActionID.MakeSpell(AID.DivineFlood), new AOEShapeCircle(6)) { }
}

public class A30TrashPack1States : StateMachineBuilder
{
    public A30TrashPack1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<PelagicCleaver1>()
            .ActivateOnEnter<PelagicCleaver2>()
            .ActivateOnEnter<PelagicCleaver1Hint>()
            .ActivateOnEnter<PelagicCleaver2Hint>()
            .ActivateOnEnter<WaterFlood>()
            .ActivateOnEnter<DivineFlood>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDestroyed) && module.Enemies(OID.Triton).All(e => e.IsDead) && module.Enemies(OID.WaterSprite).All(e => e.IsDead) && module.Enemies(OID.WaterSprite).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12478)]
public class A30TrashPack1 : BossModule
{
    public A30TrashPack1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-800, -800), 20)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var e in Enemies(OID.Boss))
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.Triton))
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.DivineSprite))
            Arena.Actor(e, ArenaColor.Enemy);
        foreach (var e in Enemies(OID.WaterSprite))
            Arena.Actor(e, ArenaColor.Enemy);
    }
}
