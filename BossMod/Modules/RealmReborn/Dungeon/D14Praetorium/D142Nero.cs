namespace BossMod.RealmReborn.Dungeon.D14Praetorium.D142Nero;

public enum OID : uint
{
    Boss = 0x3873, // x1
    MagitekDeathClaw = 0x3874, // spawn during fight
    Helper = 0x233C, // x8
};

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    IronUprising = 28482, // Boss->self, 3.0s cast, range 7 120-degree cone aoe (knockback 12)
    SpineShatter = 28483, // Boss->player, 5.0s cast, single-target tankbuster
    Teleport = 28475, // Boss->location, no cast, single-target
    AugmentedSuffering = 28476, // Boss->self, 5.0s cast, knockback 12
    AugmentedShatter = 28477, // Boss->player, 5.0s cast, range 6 circle stack
    AugmentedUprising = 28478, // Boss->self, 7.0s cast, range 45 90-degree cone aoe
    Activate = 28479, // Boss->self, 3.0s cast, single-target, visual (spawn claw)
    TheHand = 28480, // MagitekDeathClaw->player, no cast, single-target (autoattack with knockback 20)
    WheelOfSuffering = 28481, // Boss->self, 3.5s cast, range 7 circle aoe (knockback 12)
};

class IronUprising : Components.SelfTargetedAOEs
{
    public IronUprising() : base(ActionID.MakeSpell(AID.IronUprising), new AOEShapeCone(7, 60.Degrees())) { }
}

class SpineShatter : Components.SingleTargetCast
{
    public SpineShatter() : base(ActionID.MakeSpell(AID.SpineShatter)) { }
}

class AugmentedSuffering : Components.KnockbackFromCastTarget
{
    public AugmentedSuffering() : base(ActionID.MakeSpell(AID.AugmentedSuffering), 12) { }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(module.Bounds.Center, module.Bounds.HalfSize - Distance), Casters[0].CastInfo!.NPCFinishAt);
    }
}

class AugmentedShatter : Components.StackWithCastTargets
{
    public AugmentedShatter() : base(ActionID.MakeSpell(AID.AugmentedShatter), 6, 4) { }
}

class AugmentedUprising : Components.SelfTargetedAOEs
{
    public AugmentedUprising() : base(ActionID.MakeSpell(AID.AugmentedUprising), new AOEShapeCone(45, 45.Degrees())) { }
}

class WheelOfSuffering : Components.SelfTargetedAOEs
{
    public WheelOfSuffering() : base(ActionID.MakeSpell(AID.WheelOfSuffering), new AOEShapeCircle(7)) { }
}

class D142NeroStates : StateMachineBuilder
{
    public D142NeroStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronUprising>()
            .ActivateOnEnter<SpineShatter>()
            .ActivateOnEnter<AugmentedSuffering>()
            .ActivateOnEnter<AugmentedShatter>()
            .ActivateOnEnter<AugmentedUprising>()
            .ActivateOnEnter<WheelOfSuffering>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 16, NameID = 2135)]
public class D142Nero : BossModule
{
    public D142Nero(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-164, 0), 20)) { }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.MagitekDeathClaw => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var claw in Enemies(OID.MagitekDeathClaw))
            Arena.Actor(claw, ArenaColor.Danger);
    }
}
