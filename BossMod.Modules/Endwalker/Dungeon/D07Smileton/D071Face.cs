namespace BossMod.Endwalker.Dungeon.D07Smileton.D071Face;

public enum OID : uint
{
    Boss = 0x34CF, // R12.500, x1
    Helper = 0x233C, // R0.500, Helper type
    RelativelySmallFace = 0x34D0, // R3.060, x8
}

public enum AID : uint
{
    AutoAttack = 26433, // Boss->player, no cast, single-target
    LinesOfFire = 26421, // Boss->self, 3.0s cast, single-target, visual
    MixedFeelings = 26424, // Helper->self, 7.0s cast, range 60 width 2 rect
    FrownyFace = 26422, // RelativelySmallFace->self, 7.0s cast, range 45 width 6 rect, applies Frowny Face
    SmileyFace = 26423, // RelativelySmallFace->self, 7.0s cast, range 45 width 6 rect, applies Smiley Face
    UpsideDown = 26425, // Boss->self, 3.0s cast, single-target, outer faces swap colour
    FaceVisual1 = 26426, // RelativelySmallFace->self, no cast, single-target
    FaceVisual2 = 26427, // RelativelySmallFace->self, no cast, single-target
    FaceVisual3 = 26428, // RelativelySmallFace->self, no cast, single-target
    FaceVisual4 = 26429, // RelativelySmallFace->self, no cast, single-target
    OffMyLawn = 26430, // Boss->self, 5.0s cast, single-target, visual
    OffMyLawnKnockback = 27742, // Helper->self, 5.0s cast, range 31 width 30 rect, knockback 8 forward
    TemperTemper = 26432, // Helper->player, 5.0s cast, range 5 circle spread
    TempersFlare = 26435, // Boss->self, 5.0s cast, range 60 circle, raidwide
}

public enum SID : uint
{
    SmileyFace = 2763, // RelativelySmallFace->player, extra=0x1
    FrownyFace = 2764, // RelativelySmallFace->player, extra=0x1/0x2
    DownForTheCount = 783, // RelativelySmallFace->player, extra=0xEC7, two of the same face
}

public enum TetherID : uint
{
    Face = 169, // RelativelySmallFace->Boss
}

class MixedFeelings(BossModule module) : Components.StandardAOEs(module, AID.MixedFeelings, new AOEShapeRect(60, 1));

// TODO: unverified whether the face casts already use the swapped AID after Upside Down
class FaceLines(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(45, 3);
    private readonly List<(Actor caster, bool smiley)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var hasSmiley = actor.FindStatus(SID.SmileyFace) != null;
        var hasFrowny = actor.FindStatus(SID.FrownyFace) != null;
        foreach (var (c, smiley) in _casters)
        {
            var risky = smiley ? hasSmiley : hasFrowny;
            yield return new(_shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), risky ? 0 : ArenaColor.SafeFromAOE, risky);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SmileyFace or AID.FrownyFace)
            _casters.Add((caster, (AID)spell.Action.ID == AID.SmileyFace));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SmileyFace or AID.FrownyFace)
            _casters.RemoveAll(c => c.caster == caster);
    }
}

class OffMyLawn(BossModule module) : Components.KnockbackFromCastTarget(module, AID.OffMyLawnKnockback, 8, shape: new AOEShapeRect(31, 15), kind: Kind.DirForward, stopAtWall: true);
class TemperTemper(BossModule module) : Components.SpreadFromCastTargets(module, AID.TemperTemper, 5);
class TempersFlare(BossModule module) : Components.RaidwideCast(module, AID.TempersFlare);

class D071FaceStates : StateMachineBuilder
{
    public D071FaceStates(BossModule module) : base(module)
    {
        // TODO: Heart on Fire IV tankbuster AID unknown
        TrivialPhase()
            .ActivateOnEnter<MixedFeelings>()
            .ActivateOnEnter<FaceLines>()
            .ActivateOnEnter<OffMyLawn>()
            .ActivateOnEnter<TemperTemper>()
            .ActivateOnEnter<TempersFlare>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 794, NameID = 10331)]
public class D071Face(ModuleInit init) : BossModule(init, new(-45, -20), new ArenaBoundsSquare(20));
