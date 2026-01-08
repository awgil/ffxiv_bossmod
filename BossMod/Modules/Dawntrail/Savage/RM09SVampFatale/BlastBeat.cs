namespace BossMod.Dawntrail.Savage.RM09SVampFatale;

class VampStomp(BossModule module) : Components.StandardAOEs(module, AID.VampStompCast, 10);

class BlastBeat(BossModule module) : Components.GenericAOEs(module, AID.BlastBeatBat)
{
    class Bat
    {
        public required Actor Actor;
        public WPos Predicted;
        public DateTime Activation;
        public int Seq;
    }

    private readonly List<Bat> _bats = [];

    private int _seq;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var bat in _bats.Where(b => b.Seq <= _seq + 1))
            yield return new AOEInstance(new AOEShapeCircle(8), bat.Predicted, default, bat.Activation, Color: bat.Seq == _seq ? ArenaColor.Danger : ArenaColor.AOE, Risky: bat.Seq == _seq);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.VampetteFatale && id == 0x11D1)
        {
            var angleSign = actor.Rotation.ToDirection().OrthoL().Dot(actor.DirectionTo(Arena.Center)) > 0 ? 1 : -1;
            var dir = actor.Position - Arena.Center;
            var dist = dir.Length();
            var seq = dist < 10 ? 1 : dist < 16 ? 2 : 3;

            var advance = angleSign * (dist < 10 ? 88.Degrees() : 90.Degrees());
            var predicted = dir.Rotate(advance);

            var delay = 9.3f + 3.5f * (seq - 1);

            _bats.Add(new() { Actor = actor, Predicted = Arena.Center + predicted, Activation = WorldState.FutureTime(delay), Seq = seq });
            _bats.SortByReverse(b => b.Seq);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _bats.FirstOrDefault(b => b.Actor == caster) is { } bat)
        {
            bat.Predicted = spell.LocXZ;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (NumCasts is 2 or 5)
                _seq++;
            _bats.RemoveAll(b => b.Actor == caster);
        }

        if ((AID)spell.Action.ID == AID.VampStomp)
            _seq++;
    }
}

class BatRing(BossModule module) : BossComponent(module)
{
    private DateTime _start;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BatRing)
            _start = WorldState.CurrentTime;
    }

    public override void Update()
    {
        if (_start != default && WorldState.CurrentTime > _start.AddSeconds(16))
            _start = default;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_start != default)
        {
            var dt = WorldState.CurrentTime - _start;
            var radius = dt.TotalSeconds * 2;
            Arena.AddCircle(Arena.Center, (float)radius, ArenaColor.Object, 1);
        }
    }
}

class CurseOfTheBombpyre(BossModule module) : Components.GenericStackSpread(module, alwaysShowSpreads: true)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.CurseOfTheBombpyre)
            Spreads.Add(new(actor, 8, DateTime.MaxValue));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.CurseOfTheBombpyre)
            Spreads.RemoveAll(s => s.Target == actor);
    }
}
