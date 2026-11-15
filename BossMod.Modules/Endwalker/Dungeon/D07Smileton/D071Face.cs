namespace BossMod.Endwalker.Dungeon.D07Smileton.D071Face;

public enum OID : uint
{
    Boss = 0x34CF, // R10.000, x1
    RelativelySmallFace = 0x34D0, // R1.000, x8
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 26433, // Boss->player, no cast, single-target
    LinesOfFire = 26421, // Boss->self, 3.0s cast, single-target, visual (tethers faces)
    FrownyFace = 26422, // RelativelySmallFace->self, 7.0s cast, range 45 width 6 rect
    SmileyFace = 26423, // RelativelySmallFace->self, 7.0s cast, range 45 width 6 rect
    MixedFeelings = 26424, // Helper->self, 7.0s cast, range 60 width 2 rect
    UpsideDown = 26425, // Boss->self, 3.0s cast, single-target, visual (faces flip)
    FlipVisual1 = 26426, // RelativelySmallFace->self, no cast, single-target
    FlipVisual2 = 26427, // RelativelySmallFace->self, no cast, single-target
    FlipVisual3 = 26428, // RelativelySmallFace->self, no cast, single-target
    FlipVisual4 = 26429, // RelativelySmallFace->self, no cast, single-target
    OffMyLawn = 26430, // Boss->self, 5.0s cast, single-target, visual
    OffMyLawnKnockback = 27742, // Helper->self, 5.0s cast, range 31 width 30 rect, knockback 8 forward
    TemperTemper = 26432, // Helper->player, 5.0s cast, range 5 circle spread
    TempersFlare = 26435, // Boss->self, 5.0s cast, range 60 circle, raidwide
    HeartOnFireIV = 26434, // Boss->player, 5.0s cast, single-target, tankbuster
}

public enum SID : uint
{
    SmileyFace = 2763, // RelativelySmallFace->player, extra=stacks
    FrownyFace = 2764, // RelativelySmallFace->player, extra=stacks
    DownForTheCount = 783, // RelativelySmallFace->player, extra=0xEC7, second hit of the same face
}

public enum IconID : uint
{
    TemperTemper = 96, // player
}

class Expressions(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor caster, bool smiley)> _faces = [];
    private readonly List<Actor> _gaps = [];

    private static readonly AOEShapeRect _face = new(45, 3);
    private static readonly AOEShapeRect _gap = new(60, 1);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var smiley = actor.FindStatus(SID.SmileyFace) != null;
        var frowny = actor.FindStatus(SID.FrownyFace) != null;
        foreach (var (c, isSmiley) in _faces)
            if (isSmiley ? smiley : frowny)
                yield return new(_face, c.CastInfo!.LocXZ, c.CastInfo.Rotation, Module.CastFinishAt(c.CastInfo));
        foreach (var c in _gaps)
            yield return new(_gap, c.CastInfo!.LocXZ, c.CastInfo.Rotation, Module.CastFinishAt(c.CastInfo));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_faces.Count == 0)
            return;
        if (actor.FindStatus(SID.SmileyFace) != null)
            hints.Add("Avoid smiley faces!", false);
        else if (actor.FindStatus(SID.FrownyFace) != null)
            hints.Add("Avoid frowny faces!", false);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SmileyFace:
            case AID.FrownyFace:
                _faces.Add((caster, (AID)spell.Action.ID == AID.SmileyFace));
                break;
            case AID.MixedFeelings:
                _gaps.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SmileyFace:
            case AID.FrownyFace:
                _faces.RemoveAll(f => f.caster == caster);
                break;
            case AID.MixedFeelings:
                _gaps.Remove(caster);
                break;
        }
    }
}

class OffMyLawn(BossModule module) : Components.KnockbackFromCastTarget(module, AID.OffMyLawnKnockback, 8, shape: new AOEShapeRect(31, 15), kind: Kind.DirForward, stopAtWall: true);
class TemperTemper(BossModule module) : Components.SpreadFromCastTargets(module, AID.TemperTemper, 5);
class HeartOnFireIV(BossModule module) : Components.SingleTargetCast(module, AID.HeartOnFireIV);
class TempersFlare(BossModule module) : Components.RaidwideCast(module, AID.TempersFlare);

class D071FaceStates : StateMachineBuilder
{
    public D071FaceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Expressions>()
            .ActivateOnEnter<OffMyLawn>()
            .ActivateOnEnter<TemperTemper>()
            .ActivateOnEnter<TempersFlare>()
            .ActivateOnEnter<HeartOnFireIV>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 794, NameID = 10331)]
public class D071Face(ModuleInit init) : BossModule(init, new(-45, -20), new ArenaBoundsSquare(15));
