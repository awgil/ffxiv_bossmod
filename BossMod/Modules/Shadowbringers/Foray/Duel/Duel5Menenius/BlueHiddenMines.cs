namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

internal class BlueHiddenMines : Components.GenericTowers
{
    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (Towers.Count > 0)
        {
            hints.Add("Soak the mine!");
        }
    }
}
