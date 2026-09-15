namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke;

class FlukeTyphoon(BossModule module) : Components.CastCounter(module, AID.FlukeTyphoonAOE);

class FlukeTyphoonBurst(BossModule module) : Components.GenericTowers(module)
{
    public override void OnMapEffect(byte index, uint state)
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
                Towers.Add(new(Module.Center + offset, 4));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NBurst or AID.SBurst or AID.NBigBurst or AID.SBigBurst)
        {
            Towers.Clear();
            ++NumCasts;
        }
    }
}
