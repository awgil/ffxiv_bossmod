namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D012Drowsie;

public enum OID : uint
{
    Boss = 0x4195, // R5.0

    Apollyon = 0x41B9, // R7.0
    IhuykatumuIvy = 0x419C, // R4.2-8.4
    BlueClot = 0x4197, // R2.0
    GreenClot = 0x4196, // R3.5
    RedClot = 0x4198, // R1.3
    Mimiclot1 = 0x419B, // R0.8
    Mimiclot2 = 0x41A0, // R1.2
    Mimiclot3 = 0x4199, // R1.75
    Mimiclot4 = 0x41A1, // R1.2
    Mimiclot5 = 0x419A, // R1.2
    Mimiclot6 = 0x419F, // R1.75
    Helper = 0x233
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Mimiclot1/Mimiclot3/Mimiclot4/Mimiclot6/Mimiclot2/Mimiclot5->player, no cast, single-target

    Uppercut = 39132, // Boss->player, 5.0s cast, single-target

    Sow = 36476, // Boss->self, 3.0s cast, single-target // spawn seeds

    DrowsyDance = 36477, // Boss->self, 3.5s cast, single-target

    Arise = 36478, // IhuykatumuIvy->self, 3.0s cast, range 8 circle

    Wallop1 = 36479, // IhuykatumuIvy->self, 7.0s cast, range 40 width 10 rect
    Wallop2 = 36482, // IhuykatumuIvy->self, 7.0s cast, range 40 width 16 rect

    Visual1 = 36480, // Helper->IhuykatumuIvy, no cast, single-target
    Visual2 = 36481, // Boss->self, no cast, single-target
    Visual3 = 36484, // BlueClot/GreenClot/RedClot->location, no cast, single-target
    Visual4 = 36762, // Boss->self, no cast, single-target

    Sneeze = 36475, // Boss->self, 5.0s cast, range 60 150.000-degree cone
    Spit = 36483, // Boss->self, 5.0s cast, single-target

    Metamorphosis1 = 36524, // BlueClot->self, 2.0s cast, single-target
    Metamorphosis2 = 36523, // GreenClot->self, 2.0s cast, single-target
    Metamorphosis3 = 36525, // RedClot->self, 2.0s cast, single-target

    FlagrantSpread1 = 36522, // Mimiclot5/Mimiclot2->player, 5.0s cast, range 6 circle
    FlagrantSpread2 = 36485, // Mimiclot3/Mimiclot6->self, 5.0s cast, range 6 circle
}

class Uppercut(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Uppercut));
class Arise(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Arise), new AOEShapeCircle(8));
class Wallop1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Wallop1), new AOEShapeRect(40, 5));
class Wallop2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Wallop2), new AOEShapeRect(40, 8));
class SelfTargetSneezeedAOEs(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Sneeze), new AOEShapeCone(60, 75.Degrees()));
class FlagrantSpread1(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FlagrantSpread1), 6);
class FlagrantSpread2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlagrantSpread2), new AOEShapeCircle(6));

class D012DrowsieStates : StateMachineBuilder
{
    public D012DrowsieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Uppercut>()
            .ActivateOnEnter<Arise>()
            .ActivateOnEnter<Wallop1>()
            .ActivateOnEnter<Wallop2>()
            .ActivateOnEnter<SelfTargetSneezeedAOEs>()
            .ActivateOnEnter<FlagrantSpread1>()
            .ActivateOnEnter<FlagrantSpread2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12716)]
public class D012Drowsie(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    private static readonly List<Shape> union = [new Circle(new(80, 53), 19.5f)];
    private static readonly List<Shape> difference = [new Rectangle(new(65.5f, 38), 20, 1.75f, 130.Degrees()), new Rectangle(new(80, 74), 20, 2)];
    public static readonly ArenaBoundsComplex DefaultBounds = new(union, difference);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Mimiclot1))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Mimiclot2))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Mimiclot3))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Mimiclot4))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Mimiclot5))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Mimiclot6))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
