namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

class MistralSong : Components.GenericLineOfSightAOE
{
    private WPos _predictedPosition;

    public MistralSong(WPos predictedPosition) : base(ActionID.MakeSpell(AID.MistralSong), 31.7f, true)
    {
        _predictedPosition = predictedPosition;
    }

    public override void Init(BossModule module)
    {
        Modify(_predictedPosition, ActiveBlockers(module));
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(caster.Position, ActiveBlockers(module));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(null, ActiveBlockers(module));
    }

    private IEnumerable<(WPos, float)> ActiveBlockers(BossModule module) => module.Enemies(OID.Monolith).Where(a => !a.IsDead).Select(a => (a.Position, a.HitboxRadius - 0.5f));
}

class MistralSong1 : MistralSong
{
    public MistralSong1() : base(new(0, -13)) { }
}

class MistralSong2 : MistralSong
{
    public MistralSong2() : base(new(13, 0)) { }
}
