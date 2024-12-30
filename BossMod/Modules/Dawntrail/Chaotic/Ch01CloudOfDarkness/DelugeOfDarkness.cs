namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class DelugeOfDarkness1(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCustom _shape = new(Ch01CloudOfDarkness.Phase1Bounds.Clipper.Difference(new(CurveApprox.Rect(new(100, 0), new(0, 100))), new(Ch01CloudOfDarkness.Phase1Bounds.Poly)));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DelugeOfDarkness1)
            _aoe = new(_shape, Module.Center, default, Module.CastFinishAt(spell));
    }
}

// envcontrols:
// 00 = main bounds telegraph
// - 00200010 - phase 1
// - 00020001 - phase 2
// - 00040004 - remove telegraph (note that actual bounds are controlled by something else!)
// 02 = outer ring
// - 00020001 - become dangerous
// - 00080004 - restore to normal
// 03-1E = mid squares
// - 08000001 - init
// - 00200010 - become occupied
// - 02000001 - become free
// - 00800040 - player is standing for too long, will break soon
// - 00080004 - break
// - 00020001 - repair
// - arrangement:
//      04             0B
//   03 05 06 07 0E 0D 0C 0A
//      08             0F
//      09             10
//      17             1E
//      16             1D
//   11 13 14 15 1C 1B 1A 18
//      12             19
class DelugeOfDarkness2(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCustom _shape = new(Ch01CloudOfDarkness.Phase2Bounds.Clipper.Difference(new(CurveApprox.Rect(new(100, 0), new(0, 100))), new(Ch01CloudOfDarkness.Phase2Bounds.Poly)));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DelugeOfDarkness2)
            _aoe = new(_shape, Module.Center, default, Module.CastFinishAt(spell));
    }
}
