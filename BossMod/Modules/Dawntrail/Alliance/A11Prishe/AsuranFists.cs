namespace BossMod.Dawntrail.Alliance.A11Prishe;

class AsuranFists(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AsuranFists)
        {
            Towers.Add(new(caster.Position, 6, 8, 24, default, Module.CastFinishAt(spell, 0.5f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AsuranFistsAOE1 or AID.AsuranFistsAOE2 or AID.AsuranFistsAOE3)
        {
            ++NumCasts;
            if ((AID)spell.Action.ID == AID.AsuranFistsAOE3)
                Towers.Clear();
        }
    }
}
