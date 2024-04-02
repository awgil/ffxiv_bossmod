namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke;

class FlukeTyphoon : Components.CastCounter
{
    public FlukeTyphoon() : base(ActionID.MakeSpell(AID.FlukeTyphoonAOE)) { }
}

class FlukeTyphoonBurst : Components.GenericTowers
{
    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (state == 0x00020001)
        {
            WDir offset = index switch
            {
                0 => new(-10, -15),
                1 => new(-14, 0),
                2 => new(-10, +15),
                3 => new(+10, -15),
                4 => new(+14, 0),
                5 => new(+10, +15),
                _ => default
            };
            if (offset != default)
                Towers.Add(new(module.Bounds.Center + offset, 4));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NBurst or AID.SBurst or AID.NBigBurst or AID.SBigBurst)
        {
            Towers.Clear();
            ++NumCasts;
        }
    }
}
