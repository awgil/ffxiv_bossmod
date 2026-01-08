namespace BossMod.Dawntrail.Savage.RM09SVampFatale;

public class AOETimeline
{
    public record struct AOE(WPos Origin, Angle Rotation, float TimeSincePrevious);

    public readonly List<AOE> Sequence = [];
    public DateTime Last;

    public void Advance(DateTime now)
    {
        Last = now;
        if (Sequence.Count > 0)
            Sequence.RemoveAt(0);
    }
}

class GravegrazerHorizontal : Components.GenericAOEs
{
    private readonly AOETimeline _seqNorth;
    private readonly AOETimeline _seqSouth;

    public GravegrazerHorizontal(BossModule module) : base(module, AID.GravegrazerBig)
    {
        var dist = 35.16f / 11f;
        _seqNorth = new();
        _seqSouth = new();

        WPos startN = new(82, 90);
        WPos startN2 = new(118, 90);
        var angleN = 90.Degrees();
        WPos startS = new(118, 110);
        WPos startS2 = new(82, 110);
        var angleS = -90.Degrees();

        for (var i = 0; i < 12; i++)
        {
            var delay = i == 0 ? 4.2f : 1.125f;
            _seqNorth.Sequence.Add(new(startN + angleN.ToDirection() * dist * i, angleN, delay));
            _seqSouth.Sequence.Add(new(startS + angleS.ToDirection() * dist * i, angleS, delay));
        }

        for (var j = 0; j < 2; j++)
        {
            for (var i = 0; i < 12; i++)
            {
                var delay = i == 0 ? 1.8f : 1.125f;
                _seqNorth.Sequence.Add(new(startN2 + angleN.ToDirection() * dist * -i, -angleN, delay));
                _seqSouth.Sequence.Add(new(startS2 + angleS.ToDirection() * dist * -i, -angleS, delay));
            }

            for (var i = 0; i < 12; i++)
            {
                var delay = i == 0 ? 1.8f : 1.125f;
                _seqNorth.Sequence.Add(new(startN + angleN.ToDirection() * dist * i, angleN, delay));
                _seqSouth.Sequence.Add(new(startS + angleS.ToDirection() * dist * i, angleS, delay));
            }
        }

        _seqNorth.Last = _seqSouth.Last = WorldState.CurrentTime;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var last = _seqNorth.Last;
        foreach (var (s, i) in _seqNorth.Sequence.Take(4).Select((s, i) => (s, i)).Reverse())
        {
            last = last.AddSeconds(s.TimeSincePrevious);
            yield return new AOEInstance(new AOEShapeRect(10, 2.5f), s.Origin, s.Rotation, last, i == 0 ? ArenaColor.Danger : ArenaColor.AOE);
        }
        last = _seqSouth.Last;
        foreach (var (s, i) in _seqSouth.Sequence.Take(4).Select((s, i) => (s, i)).Reverse())
        {
            last = last.AddSeconds(s.TimeSincePrevious);
            yield return new AOEInstance(new AOEShapeRect(10, 2.5f), s.Origin, s.Rotation, last, i == 0 ? ArenaColor.Danger : ArenaColor.AOE);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var l = caster.Position.Z < Arena.Center.Z ? _seqNorth : _seqSouth;
            l.Advance(WorldState.CurrentTime);
        }
    }
}

class GravegrazerVertical : Components.GenericAOEs
{
    private readonly AOETimeline _seqWest = new();
    private readonly AOETimeline _seqEast;

    public GravegrazerVertical(BossModule module) : base(module, AID.GravegrazerSmall)
    {
        _seqEast = new();
        var startE = new WPos(107.5f, 80);

        for (var i = 0; i < 14; i++)
            _seqEast.Sequence.Add(new(startE + new WDir(0, i * 3.07f), default, 1.1f));

        _seqEast.Sequence.Add(new(new(104.56f, 119.9f), -90.Degrees(), 1.1f));

        for (var i = 0; i < 13; i++)
            _seqEast.Sequence.Add(new(new WPos(102.5f, 119.168f - 3.1f * i), 180.Degrees(), 1.1f));

        for (var i = 0; i < 2; i++)
            _seqEast.Sequence.Add(new(new WPos(103.4f + 3 * i, 80.1f), 90.Degrees(), 1.1f));

        for (var i = 0; i < 13; i++)
            _seqEast.Sequence.Add(new(new WPos(107.5f, 81.83f + 3.1f * i), default, 1.1f));

        for (var i = 0; i < 2; i++)
            _seqEast.Sequence.Add(new(new WPos(105.57f - 2.8f * i, 119.9f), -90.Degrees(), 1.1f));

        for (var i = 0; i < 13; i++)
            _seqEast.Sequence.Add(new(new WPos(102.5f, 117.16f - 3.1f * i), 180.Degrees(), 1.1f));

        _seqEast.Sequence.Add(new(new WPos(105.3f, 80.1f), 90.Degrees(), 1.1f));

        for (var i = 0; i < 5; i++)
            _seqEast.Sequence.Add(new(new WPos(107.5f, 80.7f + 3 * i), default, 1.1f));

        _seqWest = new();

        foreach (var item in _seqEast.Sequence)
        {
            var dir = Arena.Center - item.Origin;
            _seqWest.Sequence.Add(new(Arena.Center + dir, item.Rotation + 180.Degrees(), item.TimeSincePrevious));
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var last = _seqEast.Last;
        foreach (var (s, i) in _seqEast.Sequence.Take(4).Select((s, i) => (s, i)).Reverse())
        {
            last = last.AddSeconds(s.TimeSincePrevious);
            yield return new AOEInstance(new AOEShapeRect(5, 2.5f), s.Origin, s.Rotation, last, i == 0 ? ArenaColor.Danger : ArenaColor.AOE);
        }
        last = _seqWest.Last;
        foreach (var (s, i) in _seqWest.Sequence.Take(4).Select((s, i) => (s, i)).Reverse())
        {
            last = last.AddSeconds(s.TimeSincePrevious);
            yield return new AOEInstance(new AOEShapeRect(5, 2.5f), s.Origin, s.Rotation, last, i == 0 ? ArenaColor.Danger : ArenaColor.AOE);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var l = caster.Position.X < Arena.Center.X ? _seqWest : _seqEast;
            l.Advance(WorldState.CurrentTime);
        }
    }
}

class Electrocution(BossModule module) : Components.StandardAOEs(module, AID.Electrocution, 3);

class ElectroPuddle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Actor, DateTime Spawn)> _puddles = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (p, spawn) in _puddles)
        {
            var elapsed = spawn > WorldState.CurrentTime ? 0 : (WorldState.CurrentTime - spawn).TotalSeconds;
            var radius = 3 + (float)elapsed / 2;
            yield return new(new AOEShapeCircle(radius), p.Position);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Electrocution)
            _puddles.Add((actor, DateTime.MaxValue));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        var ix = _puddles.FindIndex(p => p.Actor.InstanceID == actor.InstanceID);

        if (ix >= 0)
        {
            if (state == 0x00100020)
                _puddles.Ref(ix).Spawn = WorldState.CurrentTime;
            else if (state == 0x00040008)
                _puddles.RemoveAt(ix);
        }
    }
}

class Plummet(BossModule module) : Components.CastTowers(module, AID.Plummet, 3, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            for (var i = 0; i < Towers.Count; i++)
                Towers.Ref(i).ForbiddenSoakers = Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask();
        }
    }
}

class DeadlyDoornail(BossModule module) : Components.Adds(module, (uint)OID.DeadlyDoornail, 0, forbidDots: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // TODO: better priorities for melees
        // if we set higher prio here, other targets (like flail) will be skipped in AOE target counting functions which can mess with autorotation
        if (actor.Class.GetRole() is Role.Healer or Role.Ranged)
            foreach (var d in ActiveActors)
                hints.SetPriority(d, 2);
    }
}
class FatalFlail(BossModule module) : Components.Adds(module, (uint)OID.FatalFlail, 0, forbidDots: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // see above
        if (actor.Class.GetRole() is Role.Healer or Role.Ranged)
            foreach (var d in ActiveActors)
                hints.SetPriority(d, 1);
    }
}
