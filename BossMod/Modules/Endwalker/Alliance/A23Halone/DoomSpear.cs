namespace BossMod.Endwalker.Alliance.A23Halone;

class DoomSpear : Components.GenericTowers
{
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DoomSpearAOE1 or AID.DoomSpearAOE2 or AID.DoomSpearAOE3)
            Towers.Add(new(caster.Position, 6, 8, int.MaxValue));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DoomSpearAOE1 or AID.DoomSpearAOE2 or AID.DoomSpearAOE3)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
            ++NumCasts;
        }
    }
}
