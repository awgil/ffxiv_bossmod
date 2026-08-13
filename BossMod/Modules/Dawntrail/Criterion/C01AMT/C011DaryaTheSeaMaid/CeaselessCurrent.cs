namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

class CeaselessCurrent(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8f, 20f), (uint)AID.CeaselessCurrent1)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CeaselessCurrent1)
        {
            Lines.Add(new(spell.LocXZ, 8f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 2.1d, 5, 2, spell.Rotation.ToDirection().Rounded().ToAngle()));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CeaselessCurrent1 or (uint)AID.CeaselessCurrent2)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (ix >= 0)
            {
                AdvanceLine(Lines[ix], Lines[ix].Next);
            }
        }
    }
}