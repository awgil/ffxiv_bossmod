namespace BossMod.Dawntrail.Trial.T03Everkeep;

class Actualize(BossModule module) : Components.RaidwideCast(module, AID.Actualize)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            Module.Arena.Bounds = T03Everkeep.NormalBounds;
    }
}
