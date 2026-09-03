namespace BossMod.Stormblood.Ultimate.UCOB;

class P3QuickmarchTrio(BossModule module) : BossComponent(module)
{
    public WPos RelativeNorth { get; private set; }
    private readonly WPos[] _safeSpots = new WPos[PartyState.MaxPartySize];

    public bool Active => RelativeNorth != default;
    private DateTime _diveAt;
    private bool _divesDone;

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
                var offset = (60 + order * 20).Degrees();
                var dir = dirToNorth + (left ? offset : -offset);
                _safeSpots[p.slot] = Module.Center + 20 * dir.ToDirection();
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // drop twister as close to edge as possible
        if (_diveAt != default && _safeSpots[slot] != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_safeSpots[slot], 1), _diveAt);

        // dodge twisters toward arena center; once they spawn, players should stop moving so megaflare AOEs get baited close to edge
        var twister = Module.FindComponent<P3Twister>();
        if (twister?.Predicted == true)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 17), DateTime.MaxValue);
        else if (twister?.Active == true)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 15), DateTime.MaxValue);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LunarDive)
        {
            _divesDone = true;
            _diveAt = default;
            Array.Fill(_safeSpots, default);
        }
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
        if (Stacks.Count > 0 && Module.FindComponent<P3QuickmarchTrio>() is { } qmt)
        {
            var stack = Stacks[0];
            var isTarget = stack.Target == actor || !stack.ForbiddenPlayers[slot];

            if (isTarget)
            {
                var safeDir = (qmt.RelativeNorth - Arena.Center).ToAngle() + 135.Degrees();
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center + safeDir.ToDirection() * 5, 2), Stacks[0].Activation);
            }

            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }
}

class P3MegaflarePuddle(BossModule module) : Components.StandardAOEs(module, AID.MegaflarePuddle, 6);
class P3TempestWing(BossModule module) : Components.TankbusterTether(module, AID.TempestWing, (uint)TetherID.TempestWing, 5, 7.3f)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var side in Tethers)
        {
            // green if pov player should take this tether
            var color = pc.Role == Role.Tank && side.Player.Role != Role.Tank ? ArenaColor.Safe : ArenaColor.Danger;

            // thick yellow line if pov player should pass this tether; thick green line is only used in encounters with specific tether priority (in this case it's random)
            var thickness = side.Player == pc && pc.Role != Role.Tank ? 2 : 1;

            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(side.Enemy.Position, side.Player.Position, 0xFF000000, thickness + 1);

            Arena.AddLine(side.Enemy.Position, side.Player.Position, color, thickness);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
        {
            if (Tethers.FirstOrNull(t => t.Player == actor) is { Enemy: var tetherSource })
            {
                foreach (var ally in Raid.WithoutSlot().Exclude(actor))
                {
                    hints.AddForbiddenZone(ShapeDistance.Circle(ally.Position, Radius), Activation);

                    if (ally.Role != Role.Tank)
                        // TODO: do we need to calculate the right cone width or is this good enough
                        hints.AddForbiddenZone(ShapeDistance.DonutSector(tetherSource.Position, (ally.Position - tetherSource.Position).Length(), 60, tetherSource.AngleTo(ally), 2.Degrees()), Activation);
                }
            }
            else
            {
                List<Func<WPos, float>> goal = [];

                foreach (var side in Tethers.Where(t => t.Player.Role != Role.Tank))
                    goal.Add(ShapeDistance.Union([ShapeDistance.InvertedRect(side.Enemy.Position, side.Player.Position, 1), ShapeDistance.Circle(side.Enemy.Position, 2)]));

                if (goal.Count > 0)
                    hints.AddForbiddenZone(ShapeDistance.Intersection(goal), Activation);
            }
        }

        hints.AddPredictedDamage(TetheredPlayers, Activation, AIHints.PredictedDamageType.Tankbuster);
    }
}
