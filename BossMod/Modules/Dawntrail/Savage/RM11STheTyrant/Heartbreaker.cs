namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

class HeartbreakKick(BossModule module) : Components.GenericTowers(module, AID._Weaponskill_HeartbreakKick1)
{
    public int ExpectedHits;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_HeartbreakKick)
            Towers.Add(new(Arena.Center, 4, maxSoakers: 8, activation: Module.CastFinishAt(spell, 1.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (NumCasts >= ExpectedHits)
                Towers.Clear();
        }
    }
}
