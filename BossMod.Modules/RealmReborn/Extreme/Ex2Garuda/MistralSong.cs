namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

class MistralSong : Components.GenericLineOfSightAOE
{
    private WPos _predictedPosition;

    public MistralSong(BossModule module, WPos predictedPosition) : base(module, AID.MistralSong, 31.7f, true)
    {
        _predictedPosition = predictedPosition;
        Modify(_predictedPosition, ActiveBlockers());
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(caster.Position, ActiveBlockers());
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(null, ActiveBlockers());
    }

    private IEnumerable<(WPos, float)> ActiveBlockers() => Module.Enemies(OID.Monolith).Where(a => !a.IsDead).Select(a => (a.Position, a.HitboxRadius - 0.5f));
}
class MistralSong1(BossModule module) : MistralSong(module, new(0, -13));
class MistralSong2(BossModule module) : MistralSong(module, new(13, 0));
