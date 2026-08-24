namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class WyvernsRadianceExawave(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8, 20))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WyvernsRadianceExawaveFirst)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = caster.Rotation.ToDirection() * 8,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.5f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WyvernsRadianceExawaveFirst or AID.WyvernsRadianceExawaveRest && Lines.Count > 0)
        {
            AdvanceLine(Lines[0], caster.Position);
            Lines.RemoveAll(l => l.ExplosionsLeft <= 0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (c, t, r) in ImminentAOEs())
            hints.AddForbiddenZone(Shape, c, r, t);
    }
}

class WyvernsVengeance(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WyvernsVengeanceFirst)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = caster.Rotation.ToDirection() * 8,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1.1f,
                ExplosionsLeft = 3,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WyvernsVengeanceFirst or AID.WyvernsVengeanceRest)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
        }
    }
}
class WyvernsRadianceCrystal(BossModule module) : Components.GenericAOEs(module)
{
    private bool _active;

    private readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var nextActive = DateTime.MinValue;
        foreach (var item in _predicted)
        {
            if (nextActive == default)
                nextActive = item.Activation;

            var highlight = item.Activation < nextActive.AddSeconds(1);

            yield return item with { Color = highlight ? ArenaColor.Danger : ArenaColor.AOE };
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_active)
            return;

        if ((AID)spell.Action.ID == AID.WyvernsVengeanceFirst)
        {
            _active = true;

            foreach (var big in Module.Enemies(OID.BigCrystal))
            {
                var first = big.Position.InCircle(Arena.Center, 11);
                var activate = Module.CastFinishAt(spell, first ? 1.6f : 2.7f);
                _predicted.Add(new(new AOEShapeCircle(12), big.Position, big.Rotation, activate));

                foreach (var small in Module.Enemies(OID.SmallCrystal).Where(c => c.Position.InCircle(big.Position, 14)))
                {
                    var sameTime = small.Position.InCircle(big.Position, 12);
                    _predicted.Add(new(new AOEShapeCircle(6), small.Position, small.Rotation, sameTime ? activate : activate.AddSeconds(1.2f)));
                }
            }

            _predicted.SortBy(p => p.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SmallCrystalExplosion or AID.BigCrystalExplosion)
        {
            NumCasts++;
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 0.5f));
        }
    }
}
class WyvernsVengeanceLine : Components.Exaflare
{
    private readonly List<(Actor Actor, float Radius, DateTime Explosion)> _crystals = [];

    public WyvernsVengeanceLine(BossModule module) : base(module, new AOEShapeCircle(6))
    {
        WarningText = "GTFO from aoe!";
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, t, r) in FutureAOEs())
            yield return new(Shape, c, r, t, FutureColor);

        foreach (var (c, r, e) in _crystals.Take(3))
            yield return new AOEInstance(new AOEShapeCircle(r), c.Position, default, e, Color: WorldState.FutureTime(2) > e ? ArenaColor.Danger : ArenaColor.AOE);

        foreach (var (c, t, r) in ImminentAOEs())
            yield return new(Shape, c, r, t, ImminentColor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WyvernsVengeanceLineFirst)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = caster.Rotation.ToDirection() * 8,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1.1f,
                ExplosionsLeft = 6,
                MaxShownExplosions = 2
            });

            if (Lines.Count == 6)
            {
                foreach (var (pos, activation, _) in FutureAOEs(limit: 10))
                    PredictExplosions(pos, 6, activation);

                _crystals.SortBy(c => c.Explosion);
                var c = _crystals.DistinctBy(c => c.Actor.InstanceID).ToList();
                _crystals.Clear();
                _crystals.AddRange(c);
            }
        }

        if ((AID)spell.Action.ID is AID.SmallCrystalExplosion or AID.BigCrystalExplosion)
            _crystals.RemoveAll(c => c.Actor.Position.AlmostEqual(caster.Position, 0.5f));
    }

    private void PredictExplosions(WPos source, float radius, DateTime activation, ulong excludeID = 0)
    {
        var tagged = WorldState.Actors.Where(a => a.InstanceID != excludeID && (OID)a.OID is OID.BigCrystal or OID.SmallCrystal && a.Position.InCircle(source, radius + a.HitboxRadius)).Select(a => (a, a.OID == (uint)OID.BigCrystal ? 12f : 6));
        _crystals.AddRange(tagged.Select(t => (t.a, t.Item2, activation.AddSeconds(1.7f))));
        foreach (var (c, r) in tagged)
            PredictExplosions(c.Position, r, activation.AddSeconds(1), c.InstanceID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WyvernsVengeanceLineFirst or AID.WyvernsVengeanceLineRest)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
        }
    }
}

class WyveCannonMiddle(BossModule module) : Components.Exaflare(module, new AOEShapeRect(40, 2, 40))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WyveCannonMiddle)
        {
            var rnd = caster.Rotation.ToDirection();
            Lines.Add(new()
            {
                Next = caster.Position + rnd.OrthoR() * 2,
                Advance = rnd.OrthoR() * 4,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.6f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
            Lines.Add(new()
            {
                Next = caster.Position + rnd.OrthoL() * 2,
                Advance = rnd.OrthoL() * 4,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.6f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WyveCannonMiddle)
        {
            NumCasts++;
            foreach (var l in Lines)
            {
                if (l.Next.AlmostEqual(caster.Position, 4) && l.Rotation.AlmostEqual(caster.Rotation, 0.1f))
                    AdvanceLine(l, caster.Position + l.Advance / 2);
            }
        }

        if ((AID)spell.Action.ID == AID.WyveCannonRepeat)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 0.1f));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
        }
    }
}

class WyveCannonEdge(BossModule module) : Components.Exaflare(module, new AOEShapeRect(40, 2, 40))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WyveCannonEdge)
        {
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = (Arena.Center - caster.Position).Normalized() * 4,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.6f,
                ExplosionsLeft = 10,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WyveCannonEdge or AID.WyveCannonRepeat)
        {
            // because lines cross in the center of the arena, we have to manually avoid advancing the same line twice with simultaneous cast events
            var imminentLines = Lines.Select(l => l.ExplosionsLeft).DefaultIfEmpty(-1).Max();
            if (imminentLines == -1)
                return;

            var ix = Lines.FindIndex(l => l.ExplosionsLeft == imminentLines && l.Next.AlmostEqual(caster.Position, 0.5f));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
            else
                ReportError($"Unexpected exaflare cast at {caster.Position}");
        }
    }
}
