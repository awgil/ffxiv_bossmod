namespace BossMod.Dawntrail.Savage.RM09SVampFatale;

class BloodyBondage(BossModule module) : Components.CastTowers(module, AID.BloodyBondageSolo, 4)
{
    private readonly RM09SVampFataleConfig _config = Service.Config.Get<RM09SVampFataleConfig>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (!_config.HellCellAssignments.Validate())
            return;

        if (spell.Action == WatchedAction && Towers.Count == 4)
        {
            var set = NumCasts == 0 ? 0 : 1;
            var assignments = new int[8];
            foreach (var (slot, group) in _config.HellCellAssignments.Resolve(Raid))
                assignments[group] = slot;

            var sorted = Towers.OrderByDescending(t => (t.Position - Arena.Center).ToAngle().Deg).ToList();
            // safeguard in case north tower is not perfectly centered and results in a negative angle, don't know if this can happen
            if (sorted[^1].Position.AlmostEqual(new(100, 88), 1))
            {
                sorted.Insert(0, sorted[^1]);
                sorted.RemoveAt(4);
            }

            for (var i = 0; i < 4; i++)
            {
                var towerPlayerSlot = assignments[set * 4 + i];
                var ix = Towers.IndexOf(sorted[i]);
                Towers.Ref(ix).ForbiddenSoakers = ~BitMask.Build(towerPlayerSlot);
            }
        }
    }
}

class CharnelCell(BossModule module) : BossComponent(module)
{
    IEnumerable<Actor> Cells => WorldState.Actors.Where(a => (OID)a.OID is OID.CharnelCell1 or OID.CharnelCell2 or OID.CharnelCell3 && a.IsTargetable && !a.IsDeadOrDestroyed);

    private readonly Actor?[] _cellsOrdered = new Actor?[8];
    private readonly int[] _playersOrdered = Utils.MakeArray(8, -1);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var c in Cells)
        {
            Arena.Actor(c, ArenaColor.Enemy);
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(c.Position, 4, 0xFF000000, 3);
            Arena.AddCircle(c.Position, 4, ArenaColor.Border);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var ix = (int)status.ID - (int)SID.HellInACell1;
        if (ix < 0)
            return;

        if (ix < 8)
        {
            if (Raid.TryFindSlot(actor, out var slot))
                _playersOrdered[slot] = ix;
            return;
        }

        ix -= 8;
        if (ix < 8)
            _cellsOrdered[ix] = actor;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var playerOrder = _playersOrdered[slot];

        for (var i = 0; i < 8; i++)
        {
            if (hints.FindEnemy(_cellsOrdered[i]) is { } c)
            {
                c.ForbidDOTs = true;
                if (playerOrder == i)
                {
                    c.Priority = 1;
                    hints.TemporaryObstacles.Add(ShapeContains.Donut(c.Actor.Position, 4, 100));
                }
                else
                {
                    c.Priority = AIHints.Enemy.PriorityInvincible;
                    hints.TemporaryObstacles.Add(ShapeContains.Circle(c.Actor.Position, 4));
                }
            }
        }
    }
}

// TODO: this appears to be based on tower debuff, which expires a few seconds before spread goes off, maybe verify
class UltrasonicSpreadTank(BossModule module) : Components.GenericBaitAway(module, AID.UltrasonicSpreadTank, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private DateTime _activation;
    private BitMask _towerPlayers;

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_activation == default)
            return;

        var target = Raid.WithSlot().ExcludedFromMask(_towerPlayers).Select(t => t.Item2).MinBy(t => t.Role != Role.Tank);
        if (target != null)
            CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeCone(40, 50.Degrees()), _activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UltrasonicSpreadCast)
        {
            _activation = Module.CastFinishAt(spell, 0.8f);

            foreach (var (slot, player) in Raid.WithSlot())
                if (player.FindStatus(SID.HellAwaits)?.ExpireAt > _activation)
                    _towerPlayers.Set(slot);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HellAwaits)
            _towerPlayers.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }
}

class UltrasonicSpreadOther(BossModule module) : Components.GenericBaitAway(module, AID.UltrasonicSpreadSmall)
{
    private DateTime _activation;
    private BitMask _towerPlayers;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UltrasonicSpreadCast)
        {
            _activation = Module.CastFinishAt(spell, 0.8f);

            foreach (var (slot, player) in Raid.WithSlot())
                if (player.FindStatus(SID.HellAwaits)?.ExpireAt > _activation)
                    _towerPlayers.Set(slot);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HellAwaits)
            _towerPlayers.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_activation == default)
            return;

        var cat = pc.Class.GetRole2();
        if (IsTarget(pcSlot, pc))
            Arena.AddCone(Module.PrimaryActor.Position, 40, Module.PrimaryActor.AngleTo(pc), 22.5f.Degrees(), ArenaColor.Danger);

        if (cat != Role2.Healer || _towerPlayers[pcSlot])
            DrawBait(Role2.Healer);
        if (cat != Role2.DPS || _towerPlayers[pcSlot])
            DrawBait(Role2.DPS);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default)
            return;

        if (IsTarget(slot, actor))
        {
            var cat = actor.Class.GetRole2();
            var forbidden = Raid.WithSlot().Where(p => IsForbidden(p.Item1, p.Item2, actor));
            hints.Add("Bait away from raid!", forbidden.Any(p => p.Item2.Position.InCone(Module.PrimaryActor.Position, Module.PrimaryActor.DirectionTo(actor), 22.5f.Degrees())));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation == default)
            return;

        if (IsTarget(slot, actor))
        {
            var cones = Raid.WithSlot().Where(p => IsForbidden(p.Item1, p.Item2, actor)).Select(p => ShapeContains.Cone(Module.PrimaryActor.Position, 40, Module.PrimaryActor.AngleTo(p.Item2), 22.5f.Degrees())).ToList();
            if (cones.Count > 0)
                hints.AddForbiddenZone(ShapeContains.Union(cones), _activation);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _activation == default
        ? PlayerPriority.Normal
        : IsTarget(playerSlot, player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    private bool IsTarget(int slot, Actor actor) => !_towerPlayers[slot] && actor.Class.GetRole2() != Role2.Tank;

    private bool IsForbidden(int slot, Actor actor, Actor pc) => _towerPlayers[slot] || actor.Class.GetRole2() != pc.Class.GetRole2();

    private void DrawBait(Role2 c)
    {
        if (Raid.WithSlot().ExcludedFromMask(_towerPlayers).Select(p => p.Item2).FirstOrDefault(r => r.Class.GetRole2() == c) is { } actor)
            Arena.ZoneCone(Module.PrimaryActor.Position, 0, 60, Module.PrimaryActor.AngleTo(actor), 22.5f.Degrees(), ArenaColor.AOE);
    }
}

class UltrasonicAmp(BossModule module) : Components.CastCounter(module, AID.UltrasonicAmp)
{
    private DateTime _activation;
    private BitMask _towerPlayers;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UltrasonicAmpCast)
        {
            _activation = Module.CastFinishAt(spell, 0.8f);
            foreach (var (slot, player) in Raid.WithSlot())
                if (player.FindStatus(SID.HellAwaits)?.ExpireAt > _activation)
                    _towerPlayers.Set(slot);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HellAwaits)
            _towerPlayers.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_activation == default)
            return;

        var target = IsTarget(pcSlot) ? pc : Raid.WithSlot().ExcludedFromMask(_towerPlayers).Select(p => p.Item2).FirstOrDefault();
        if (target == null)
            return;

        Arena.ZoneCone(Module.PrimaryActor.Position, 0, 40, Module.PrimaryActor.AngleTo(target), 50.Degrees(), _towerPlayers[pcSlot] ? ArenaColor.AOE : ArenaColor.SafeFromAOE);
    }

    private readonly AOEShapeCone Cone = new(40, 50.Degrees());

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default)
            return;

        if (IsTarget(slot))
        {
            hints.Add("Stack!", !Raid.WithSlot().ExcludedFromMask(_towerPlayers).Exclude(actor).InShape(Cone, Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(actor)).Any());
            hints.Add("Bait away from towers!", Raid.WithSlot().IncludedInMask(_towerPlayers).InShape(Cone, Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(actor)).Any());
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation == default)
            return;

        var cones = Raid.WithSlot().IncludedInMask(_towerPlayers).Select(p => ShapeContains.Cone(Module.PrimaryActor.Position, 40, Module.PrimaryActor.AngleTo(p.Item2), 50.Degrees())).ToList();
        if (cones.Count > 0)
            hints.AddForbiddenZone(ShapeContains.Union(cones), _activation);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _activation == default
            ? PlayerPriority.Normal
            : _towerPlayers[playerSlot]
                ? PlayerPriority.Normal
                : (_towerPlayers[pcSlot] ? PlayerPriority.Danger : PlayerPriority.Interesting);
    }

    private bool IsTarget(int slot) => !_towerPlayers[slot];
}

class UltrasonicCounter(BossModule module) : Components.CastCounterMulti(module, [AID.UltrasonicSpreadTank, AID.UltrasonicAmp]);
