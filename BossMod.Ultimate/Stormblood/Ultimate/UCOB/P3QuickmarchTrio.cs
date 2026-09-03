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
                _safeSpots[p.slot] = Module.Center + 19.5f * dir.ToDirection();
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_diveAt != default && _safeSpots[slot] != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_safeSpots[slot], 3), _diveAt);

        if (Module.FindComponent<P3Twister>() is { Predicted: true } or { Active: true })
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, 10));
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
        if (IsStackTarget(actor) && Module.FindComponent<P3QuickmarchTrio>() is { } qmt)
        {
            var safeDir = (qmt.RelativeNorth - Arena.Center).ToAngle() + 135.Degrees();
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center + safeDir.ToDirection() * 5, 2), Stacks[0].Activation);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class P3MegaflarePuddle(BossModule module) : Components.StandardAOEs(module, AID.MegaflarePuddle, 6);
class P3TempestWing(BossModule module) : Components.TankbusterTether(module, AID.TempestWing, (uint)TetherID.TempestWing, 5);
