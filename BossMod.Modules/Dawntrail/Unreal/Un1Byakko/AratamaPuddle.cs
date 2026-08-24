namespace BossMod.Dawntrail.Unreal.Un1Byakko;

class AratamaPuddleBait(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.AratamaPuddle, AID.AratamaPuddle, 4, 5.1f)
{
    private DateTime _nextSpread;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == SpreadAction && WorldState.CurrentTime > _nextSpread)
        {
            if (++NumFinishedSpreads >= 3)
                Spreads.Clear();
            else
                _nextSpread = WorldState.FutureTime(0.5f); // protection in case one target dies
        }
    }
}

class AratamaPuddleVoidzone(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.AratamaPuddle).Where(z => z.EventState != 7));
