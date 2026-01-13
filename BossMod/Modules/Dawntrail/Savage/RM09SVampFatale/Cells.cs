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

class Ultrasonic(BossModule module) : Components.UntelegraphedBait(module)
{
    public static readonly AOEShape TankShape = new AOEShapeCone(40, 50.Degrees());
    public static readonly AOEShape SquishyShape = new AOEShapeCone(40, 22.5f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UltrasonicSpreadCast)
        {
            var activate = Module.CastFinishAt(spell, 0.8f);
            var nonTowers = Raid.WithSlot().WhereActor(c => !(c.FindStatus(SID.HellAwaits)?.ExpireAt > activate));

            CurrentBaits.Add(new(Module.PrimaryActor.Position, nonTowers.WhereActor(t => t.Role == Role.Tank).Mask(), TankShape, activate, 1));
            CurrentBaits.Add(new(Module.PrimaryActor.Position, nonTowers.WhereActor(t => t.Role == Role.Healer).Mask(), SquishyShape, activate, 1));
            CurrentBaits.Add(new(Module.PrimaryActor.Position, nonTowers.WhereActor(t => t.Class.IsDD()).Mask(), SquishyShape, activate, 1));
        }

        if ((AID)spell.Action.ID == AID.UltrasonicAmpCast)
        {
            var activate = Module.CastFinishAt(spell, 0.8f);
            var nonTowers = Raid.WithSlot().WhereActor(c => !(c.FindStatus(SID.HellAwaits)?.ExpireAt > activate)).Mask();

            CurrentBaits.Add(new(Module.PrimaryActor.Position, nonTowers, TankShape, activate, 1, 2, forbiddenTargets: ~nonTowers));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UltrasonicAmp:
            case AID.UltrasonicSpreadSmall:
            case AID.UltrasonicSpreadTank:
                NumCasts++;
                if (CurrentBaits.Count > 0)
                    CurrentBaits.RemoveAt(0);
                break;
        }
    }
}

class UltrasonicCounter(BossModule module) : Components.CastCounterMulti(module, [AID.UltrasonicSpreadTank, AID.UltrasonicAmp]);
