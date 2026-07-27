namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P5ParadiseRegainedTowers(BossModule module) : Components.GenericTowers(module, (uint)AID.WingsDarkAndLightExplosion)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // TODO: implement hints for non-tanks here...
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 51 and <= 53 && state == 0x00020001u)
        {
            var dir = index switch
            {
                51 => -120f.Degrees(),
                52 => 120f.Degrees(),
                _ => default
            };
            var forbidden = Raid.WithSlot(true, true, true).WhereActor(p => p.Role == Role.Tank).Mask(); // TODO: assignments
            Towers.Add(new(Arena.Center + 7f * dir.ToDirection(), 3f, 2, 2, forbidden, WorldState.FutureTime(9.5d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            if (Towers.Count == 0)
            {
                ReportError($"Unexpected expolosion @ {caster.Position}");
            }
            else
            {
                if (!Towers[0].Position.AlmostEqual(caster.Position, 1))
                    ReportError($"Unexpected position: {caster.Position} instead of {Towers[0].Position}");
                Towers.RemoveAt(0);
            }
        }
    }
}

sealed class P5ParadiseRegainedBaits(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly WDir _relSouth = module.FindComponent<P5ParadiseRegainedTowers>() is var towers && towers?.Towers.Count > 0 ? towers.Towers[0].Position - module.Center : default;
    private Actor? _source;
    private Actor? _firstTarget;
    private AOEShapeCone? _curCleave;
    private DateTime _activation;
    private bool _tetherClosest;

    private readonly AOEShapeCone _shapeCleaveL = new(20f, 120f.Degrees(), 60f.Degrees()); // note: looks wrong on bait target with correct range...
    private readonly AOEShapeCone _shapeCleaveD = new(20f, 120f.Degrees(), -60f.Degrees());
    private readonly AOEShapeCircle _shapeTether = new(4);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null && _curCleave != null)
        {
            var cleaveTarget = NumCasts == 0 ? _firstTarget : WorldState.Actors.Find(_source.TargetID);
            if (cleaveTarget != null)
                CurrentBaits.Add(new(_source, cleaveTarget, _curCleave, _activation));
            var tetherTarget = _tetherClosest ? Raid.WithoutSlot(false, true, true).Closest(_source.Position) : Raid.WithoutSlot(false, true, true).Farthest(_source.Position);
            if (tetherTarget != null)
                CurrentBaits.Add(new(tetherTarget, tetherTarget, _shapeTether, _activation)); // +0.7s?
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ForbiddenPlayers[slot] && CurrentBaits.Count == 2 && _source != null && _firstTarget != null)
        {
            var isFirstTank = actor == _firstTarget;
            if (_source.TargetID == _firstTarget.InstanceID)
                hints.Add(isFirstTank ? "Pass aggro!" : "Taunt!"); // TODO: can a tank bait both cleaves with invuln?

            var firstTankShouldBaitTether = NumCasts > 0; // TODO: can a tank bait cleave+tether with invuln?
            var shouldBaitTether = isFirstTank == firstTankShouldBaitTether;
            if (shouldBaitTether && CurrentBaits[1].Target != actor)
                hints.Add(_tetherClosest ? "Go closer!" : "Go farther!");
        }

        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ForbiddenPlayers[slot])
        {
            // just go to the next safespot
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center + SafeOffset(slot, actor), 1f));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var safeOffset = SafeOffset(pcSlot, pc);
        if (safeOffset != default)
        {
            Arena.AddCircle(Arena.Center + safeOffset, 1f, Colors.Safe);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        (var shape, var closest) = spell.Action.ID switch
        {
            (uint)AID.WingsDarkAndLightDL => (_shapeCleaveD, true),
            (uint)AID.WingsDarkAndLightLD => (_shapeCleaveL, false),
            _ => (null, false)
        };
        if (shape != null)
        {
            ForbiddenPlayers = Raid.WithSlot(true, true, true).WhereActor(p => p.Role != Role.Tank).Mask();
            _source = caster;
            _firstTarget = WorldState.Actors.Find(caster.TargetID);
            _curCleave = shape;
            _activation = Module.CastFinishAt(spell, 0.3d);
            _tetherClosest = closest;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var nextShape = spell.Action.ID switch
        {
            (uint)AID.WingsDarkAndLightCleaveLight => _shapeCleaveD,
            (uint)AID.WingsDarkAndLightCleaveDark => _shapeCleaveL,
            _ => null
        };
        if (nextShape != null)
        {
            ++NumCasts;
            _curCleave = nextShape;
            _activation = WorldState.FutureTime(3.7d);
            _tetherClosest = !_tetherClosest;
        }
    }

    private WDir SafeOffset(int slot, Actor player)
    {
        if (_relSouth == default || _curCleave == null)
            return default; // not initialized properly

        var southDir = Angle.FromDirection(_relSouth);
        if (ForbiddenPlayers[slot])
        {
            return default; // TODO: implement hints for non-tanks
        }
        else if (player == _firstTarget)
        {
            if (NumCasts == 0)
            {
                // bait cleave, so that south is safe
                return 7f * (southDir + 2f * _curCleave.DirectionOffset).ToDirection();
            }
            else
            {
                // bait tether across south
                return (_tetherClosest ? 2f : 10f) * (southDir + 180f.Degrees()).ToDirection();
            }
        }
        else
        {
            if (NumCasts > 0)
            {
                // bait cleave, so that north is safe
                return 7f * (southDir - _curCleave.DirectionOffset).ToDirection();
            }
            else if (_tetherClosest)
            {
                // bait tether at south
                return 2f * southDir.ToDirection();
            }
            else
            {
                // bait tether at max melee at 45 degrees
                return 10f * (southDir + 0.75f * _curCleave.DirectionOffset).ToDirection();
            }
        }
    }
}
