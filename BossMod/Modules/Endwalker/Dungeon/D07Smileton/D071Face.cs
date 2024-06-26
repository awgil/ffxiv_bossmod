namespace BossMod.Endwalker.Dungeon.D07Smileton.D071Face;

public enum OID : uint
{
    Boss = 0x34CF, // R=12.5
    Helper = 0x233C,
    RelativelySmallFace = 0x34D0, // R3.060, x8
}

public enum AID : uint
{
    AutoAttack = 26433, // Boss->player, no cast, single-target
    LinesOfFire = 26421, // Boss->self, 3.0s cast, single-target

    OffMyLawn1 = 26430, // Boss->self, 5.0s cast, single-target
    OffMyLawnKnockback = 27742, // Helper->self, 5.0s cast, range 31 width 30 rect //Knockback 8

    MixedFeelings = 26424, // Helper->self, 7.0s cast, range 60 width 2 rect
    FrownyFace = 26422, // RelativelySmallFace->self, 7.0s cast, range 45 width 6 rect
    SmileyFace = 26423, // RelativelySmallFace->self, 7.0s cast, range 45 width 6 rect

    TempersFlare = 26435, // Boss->self, 5.0s cast, range 60 circle //Raidwide
    TemperTemperSpread = 26432, // Helper->player, 5.0s cast, range 5 circle //Spread mechanic

    UnknownAbility1 = 26426, // RelativelySmallFace->self, no cast, single-target
    UnknownAbility2 = 26427, // RelativelySmallFace->self, no cast, single-target
    UnknownAbility3 = 26428, // RelativelySmallFace->self, no cast, single-target
    UnknownAbility4 = 26429, // RelativelySmallFace->self, no cast, single-target
    UpsideDown = 26425, // Boss->self, 3.0s cast, single-target
}

public enum SID : uint
{
    SmileyFace = 2763, // 34D0->player, extra=0x1
    FrownyFace = 2764, // 34D0->player, extra=0x1/0x2
    DownForTheCount = 783, // 34D0->player, extra=0xEC7
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon136 = 136, // player
    Icon96 = 96, // player
    Icon137 = 137, // player
}

public enum TetherID : uint
{
    Tether169 = 169, // 34D0->Boss
}
class OffMyLawnKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.OffMyLawnKnockback), 8, kind: Kind.DirForward, stopAtWall: true);
class TemperTemperSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.TemperTemperSpread), 5);

class TempersFlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TempersFlare));

class D071FaceStates : StateMachineBuilder
{
    public D071FaceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OffMyLawnKnockback>()
            .ActivateOnEnter<TemperTemperSpread>()
            .ActivateOnEnter<TempersFlare>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 794, NameID = 10331)]
public class D071Face(WorldState ws, Actor primary) : BossModule(ws, primary, new(-45, -20), new ArenaBoundsSquare(20));
