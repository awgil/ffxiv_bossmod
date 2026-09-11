namespace BossMod.Stormblood.Ultimate.UCOB;

class P3QuickmarchTrio(BossModule module) : BossComponent(module)
{
    public WPos RelativeNorth { get; private set; }
    private readonly WPos[] _safeSpots = new WPos[PartyState.MaxPartySize];
    private readonly WPos[] _spreadSpots = new WPos[PartyState.MaxPartySize];

    public bool Active => RelativeNorth != default;
    private DateTime _diveAt;
    private bool _divesDone;
    private bool _earthshakersDone;

    public bool PuddleDodgeHint;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Active)
            Arena.ActorInsideBounds(RelativeNorth, (Arena.Center - RelativeNorth).ToAngle(), ArenaColor.Object);
        var safespot = _safeSpots[pcSlot];
        if (safespot != default)
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (_divesDone)
            return;

        if ((OID)actor.OID == OID.BahamutPrime && id == 0x1E43)
        {
            RelativeNorth = actor.Position;
            _diveAt = WorldState.FutureTime(5.1f);
            var dirToNorth = Angle.FromDirection(actor.Position - Module.Center);
            foreach (var p in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(Raid))
            {
                var left = p.group < 4;
                var order = p.group & 3;
                var offSafe = (60 + order * 20).Degrees();
                var dirSafe = dirToNorth + (left ? offSafe : -offSafe);
                _safeSpots[p.slot] = Module.Center + 20 * dirSafe.ToDirection();
                var offSpread = (90 + (order - 1.5f) * 35).Degrees();
                var dirSpread = dirToNorth + (left ? offSpread : -offSpread);
                _spreadSpots[p.slot] = Module.Center + 12 * dirSpread.ToDirection();
            }

            if (Module.FindComponent<P3BahamutPositioning>() is { } bp)
                bp.DesiredRotation = dirToNorth;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // drop twister as close to edge as possible
        if (_safeSpots[slot] != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_safeSpots[slot], 1), _diveAt);

        if (Module.FindComponent<P3Twister>() is { Predicted: true } or { Active: true } && _spreadSpots[slot] != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_spreadSpots[slot], 1));

        if (PuddleDodgeHint)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 13));

        if (_earthshakersDone && actor.InstanceID != ((UCOB)Module).BahamutPrime()?.TargetID)
            hints.AddForbiddenZone(ShapeDistance.HalfPlane(Arena.Center, (Arena.Center - RelativeNorth).Normalized()), DateTime.MaxValue);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MegaflarePuddle)
            Array.Fill(_spreadSpots, default);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LunarDive)
        {
            _divesDone = true;
            _diveAt = default;
            Array.Fill(_safeSpots, default);
        }

        if ((AID)spell.Action.ID == AID.EarthShakerAOE)
            _earthshakersDone = true;
    }
}

class P3TwistingDive(BossModule module) : Components.StandardAOEs(module, AID.TwistingDive, new AOEShapeRect(60, 4));
class P3LunarDive(BossModule module) : Components.StandardAOEs(module, AID.LunarDive, new AOEShapeRect(60, 4));
class P3MegaflareDive(BossModule module) : Components.StandardAOEs(module, AID.MegaflareDive, new AOEShapeRect(60, 6));
class P3Twister(BossModule module) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, AID.TwistingDive, 1.4f, predictBeforeSpawn: 0.8f)
{
    public bool Predicted => PredictedPositions.Count > 0;
}

class P3MegaflareSpreadStack : Components.UniformStackSpread
{
    private BitMask _stackTargets;

    public P3MegaflareSpreadStack(BossModule module) : base(module, 5, 5, 3, 3, alwaysShowSpreads: true)
    {
        AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(2.6f));
        ExtraAISpreadThreshold = 0;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
            _stackTargets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MegaflareSpread:
                Spreads.Clear();
                if (Stacks.Count > 0)
                    return;
                var stackTarget = Raid.WithSlot().IncludedInMask(_stackTargets).FirstOrDefault().Item2; // random target
                if (stackTarget != null)
                    AddStack(stackTarget, WorldState.FutureTime(4), ~_stackTargets);
                break;
            case AID.MegaflareStack:
                Stacks.Clear();
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<P3QuickmarchTrio>() is { } qmt && Stacks.Count > 0)
        {
            var stack = Stacks[0];
            var isTarget = stack.Target == actor || !stack.ForbiddenPlayers[slot];

            if (isTarget)
            {
                var safeDir = (qmt.RelativeNorth - Arena.Center).ToAngle() + 135.Degrees();
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center + safeDir.ToDirection() * 5, 2));
            }

            // everyone else should avoid the stack, it will kill healers and do ~50% to tanks
            else if (actor.Class.IsSupport())
                foreach (var (_, candidate) in Raid.WithSlot().ExcludedFromMask(stack.ForbiddenPlayers))
                    hints.AddForbiddenZone(ShapeDistance.Circle(candidate.Position, StackRadius), stack.Activation);

            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }
}

class P3MegaflarePuddle(BossModule module) : Components.StandardAOEs(module, AID.MegaflarePuddle, 6);
class P3TempestWing(BossModule module) : Components.TankbusterTether(module, AID.TempestWingAOE, (uint)TetherID.TempestWing, 5, 7.3f)
{
    // determines whether non-tank players should try to avoid tanks
    // disabled by default, since we assume the tethers spawn on random players, meaning non-tanks should plant and let tanks grab tethers from them; should be set to true after some appropriate amount of delay
    public bool EnableRaidHints;

    public DateTime TetherDeadline;

    // tethers disappear more than a full second before the AOE goes off
    private BitMask _lastTethered;
    private BitMask Targets => TetheredPlayers.Any() ? TetheredPlayers : _lastTethered;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;

        if (actor.Role == Role.Tank)
        {
            if (!TetheredPlayers[slot])
                hints.Add("Grab a tether!");
            else if (Raid.WithoutSlot().InRadiusExcluding(actor, Radius).Any() && EnableRaidHints)
                hints.Add("GTFO from raid!");
        }
        else if (EnableRaidHints)
        {
            foreach (var (_, t) in Raid.WithSlot().IncludedInMask(Targets))
            {
                if (t == actor)
                    hints.Add("Hit by tankbuster!");
                else if (actor.Position.InCircle(t.Position, Radius))
                    hints.Add("GTFO from tank!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var side in Tethers)
        {
            // green if pov player should take this tether
            var color = pc.Role == Role.Tank && side.Player.Role != Role.Tank && !TetheredPlayers[pcSlot] ? ArenaColor.Safe : ArenaColor.Danger;

            // thick yellow line if pov player should pass this tether; thick green line is only used in encounters with specific tether priority (in this case it's random)
            var thickness = side.Player == pc && pc.Role != Role.Tank ? 2 : 1;

            if (Arena.Config.ShowOutlinesAndShadows)
            {
                Arena.AddLine(side.Enemy.Position, side.Player.Position, 0xFF000000, thickness + 1);
                Arena.AddCircle(side.Player.Position, 5, 0xFF000000, 2);
            }

            Arena.AddLine(side.Enemy.Position, side.Player.Position, color, thickness);
            Arena.AddCircle(side.Player.Position, 5, ArenaColor.Danger);
        }

        if (Tethers.Count == 0)
            foreach (var (_, p) in Raid.WithSlot().IncludedInMask(Targets))
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddCircle(p.Position, 5, 0xFF000000, 2);
                Arena.AddCircle(p.Position, 5, ArenaColor.Danger);
            }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);

        if ((TetherID)tether.ID == TetherID.TempestWing)
        {
            if (TetherDeadline == default)
                TetherDeadline = Activation.AddSeconds(-1.4f);
            _lastTethered.Reset();
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        // if tether count is >2 it means that this tether just got passed and the gain event fired before the lose event
        if ((TetherID)tether.ID == TetherID.TempestWing && Tethers.Count <= 2)
            _lastTethered.Set(Raid.FindSlot(source.InstanceID));

        base.OnUntethered(source, tether);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (Targets[playerSlot])
            return PlayerPriority.Danger;

        // for tanks, other players are interesting, since tank should not clip them
        if (pc.Role == Role.Tank)
            return PlayerPriority.Normal;

        // for non-tanks, other players are irrelevant
        return PlayerPriority.Irrelevant;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
        {
            if (Tethers.FirstOrNull(t => t.Player == actor) is { Enemy: var tetherSource })
            {
                foreach (var (aslot, ally) in Raid.WithSlot().Exclude(actor))
                {
                    hints.AddForbiddenZone(ShapeDistance.Circle(ally.Position, Radius), Activation);

                    // if we walk behind another player, it will pass the tether to them
                    // the cone width doesn't really matter here; as long as the pixels are blocked, pathfinder won't try to go through them
                    if (ally.Role != Role.Tank && !TetheredPlayers[aslot])
                        hints.AddForbiddenZone(ShapeDistance.DonutSector(tetherSource.Position, (ally.Position - tetherSource.Position).Length(), 60, tetherSource.AngleTo(ally), 2.Degrees()));
                }
            }
            else if (Targets[slot])
            {
                foreach (var ally in Raid.WithoutSlot().Exclude(actor))
                    hints.AddForbiddenZone(ShapeDistance.Circle(ally.Position, Radius), Activation);
            }
            else
            {
                List<Func<WPos, float>> goal = [];

                foreach (var side in Tethers.Where(t => t.Player.Role != Role.Tank))
                {
                    var toTarget = side.Player.Position - side.Enemy.Position;
                    var ttDir = toTarget.Normalized();
                    goal.Add(ShapeDistance.PrecisePosition(WPos.Lerp(side.Player.Position, side.Enemy.Position, 0.5f), new(0, 1), 0.5f, actor.Position, 0.1f));
                }

                if (goal.Count > 0)
                    hints.AddForbiddenZone(ShapeDistance.Intersection(goal), TetherDeadline);
            }
        }
        else
        {
            foreach (var side in Tethers)
            {
                // don't steal from tank
                if (side.Player.Role == Role.Tank)
                    hints.AddForbiddenZone(ShapeDistance.Rect(side.Enemy.Position, side.Player.Position, 1), TetherDeadline);

                // don't move too close to source, or tank will be unable to grab tether
                if (side.Player == actor)
                    hints.AddForbiddenZone(ShapeDistance.Circle(side.Enemy.Position, 2));
            }

            if (EnableRaidHints)
            {
                foreach (var (_, target) in Raid.WithSlot().IncludedInMask(Targets))
                    hints.AddForbiddenZone(ShapeDistance.Circle(target.Position, 5), Activation);
            }
        }

        if (Targets.Any())
            hints.AddPredictedDamage(Targets, Activation, AIHints.PredictedDamageType.Tankbuster);
    }
}
