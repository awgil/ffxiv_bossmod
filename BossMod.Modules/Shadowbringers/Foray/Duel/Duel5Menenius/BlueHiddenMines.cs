namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class BlueHiddenMines(BossModule module) : Components.GenericTowers(module)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ActivateBlueMine)
        {
            Towers.Add(new(caster.Position, 3.6f));
        }
        else if ((AID)spell.Action.ID is AID.DetonateBlueMine)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Towers.Count > 0)
        {
            hints.Add("Soak the mine!");
        }
    }
}
