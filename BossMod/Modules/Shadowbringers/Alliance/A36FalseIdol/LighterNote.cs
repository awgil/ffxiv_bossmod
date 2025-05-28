namespace BossMod.Shadowbringers.Alliance.A36FalseIdol;

class LighterNote(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6), AID.LighterNoteRest)
{
    private readonly List<(Actor Actor, DateTime Spawn)> _indicators = [];

    public const float LockInDelay = 5.5f;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.LighterNoteEW or OID.LighterNoteNS)
            _indicators.Add((actor, WorldState.CurrentTime));
    }

    public override void Update()
    {
        for (var i = _indicators.Count - 1; i >= 0; --i)
        {
            var ind = _indicators[i];
            if (ind.Spawn.AddSeconds(LockInDelay) < WorldState.CurrentTime)
            {
                _indicators.RemoveAt(i);
                var dir = ind.Actor.OID == (uint)OID.LighterNoteEW ? new WDir(1, 0) : new WDir(0, 1);
                Lines.Add(new()
                {
                    Next = ind.Actor.Position,
                    Rotation = Angle.FromDirection(dir),
                    Advance = dir * 6,
                    NextExplosion = WorldState.FutureTime(1),
                    TimeToMove = 1,
                    ExplosionsLeft = 10,
                    MaxShownExplosions = 4
                });
                Lines.Add(new()
                {
                    Next = ind.Actor.Position,
                    Rotation = Angle.FromDirection(-dir),
                    Advance = -dir * 6,
                    NextExplosion = WorldState.FutureTime(1),
                    TimeToMove = 1,
                    ExplosionsLeft = 10,
                    MaxShownExplosions = 4
                });
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LighterNoteFirst)
        {
            NumCasts++;
            foreach (var l in Lines)
                if (l.Next.AlmostEqual(spell.TargetXZ, 0.5f))
                    AdvanceLine(l, spell.TargetXZ);
            Prune();
        }

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var l = Lines.FindIndex(l => l.Next.AlmostEqual(spell.TargetXZ, 0.5f) && l.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (l >= 0)
                AdvanceLine(Lines[l], spell.TargetXZ);
            Prune();
        }
    }

    private void Prune()
    {
        for (var i = Lines.Count - 1; i >= 0; i--)
            if (!Arena.InBounds(Lines[i].Next))
                Lines.RemoveAt(i);
    }
}

class LighterNoteSpread(BossModule module) : BossComponent(module)
{
    private readonly Actor?[] _indicators = new Actor?[PartyState.MaxAllianceSize];
    private BitMask _tagged;
    private DateTime _spawn;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.LighterNoteEW or OID.LighterNoteNS)
        {
            _spawn = WorldState.CurrentTime;
            var closest = Raid.WithSlot().ExcludedFromMask(_tagged).Closest(actor.Position);
            _indicators[closest.Item1] = actor;
            _tagged.Set(closest.Item1);
        }
    }

    public override void Update()
    {
        if (_spawn.AddSeconds(LighterNote.LockInDelay) < WorldState.CurrentTime)
        {
            Array.Fill(_indicators, null);
            _tagged.Reset();
            _spawn = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_indicators.BoundSafeAt(slot) != null)
            hints.Add("Bait away from party!", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_indicators.BoundSafeAt(slot) is { } i)
        {
            switch ((OID)i.OID)
            {
                case OID.LighterNoteEW:
                    hints.AddForbiddenZone(ShapeContains.Rect(Arena.Center, 90.Degrees(), 30, 30, 18), _spawn.AddSeconds(LighterNote.LockInDelay));
                    break;
                case OID.LighterNoteNS:
                    hints.AddForbiddenZone(ShapeContains.Rect(Arena.Center, default(Angle), 30, 30, 18), _spawn.AddSeconds(LighterNote.LockInDelay));
                    break;
            }
        }
        else
        {
            foreach (var p in _tagged.SetBits())
            {
                if (_indicators[p] is { } i2)
                    hints.AddForbiddenZone(new AOEShapeCircle(6), i2.Position, default, _spawn.AddSeconds(LighterNote.LockInDelay));
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var p in _tagged.SetBits())
        {
            if (_indicators[p] is { } i)
            {
                if (p == pcSlot)
                {
                    if (i.OID == (uint)OID.LighterNoteEW)
                        Arena.AddRect(i.Position, new WDir(1, 0), 50, 50, 6, ArenaColor.Danger);
                    else
                        Arena.AddRect(i.Position, new WDir(0, 1), 50, 50, 6, ArenaColor.Danger);
                }
                else
                {
                    Arena.AddCircle(i.Position, 6, ArenaColor.Danger);
                }
                Arena.AddLine(Raid[p]?.Position ?? i.Position, i.Position, ArenaColor.Danger);
            }
        }
    }
}
