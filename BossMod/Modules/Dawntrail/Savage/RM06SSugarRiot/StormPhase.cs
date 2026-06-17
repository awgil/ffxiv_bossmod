namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

class TasteOfLightningBait(BossModule module) : BossComponent(module)
{
    public bool Baiting = true;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightningFlash)
            Baiting = false;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Baiting)
            foreach (var p in Raid.WithoutSlot())
                Arena.AddCircle(p.Position, 3, ArenaColor.Danger);
    }
}

class LightningFlash(BossModule module) : Components.CastCounter(module, AID.LightningFlash);

class TasteOfLightningDelayed(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime Activation;
    private readonly List<WPos> Baits = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightningFlash)
        {
            Baits.AddRange(Raid.WithoutSlot().Select(r => r.Position));
            Activation = WorldState.FutureTime(3.15f);
        }

        if ((AID)spell.Action.ID == AID.TasteOfThunderDelayed)
        {
            NumCasts++;
            if (Baits.Count > 0)
            {
                Baits.RemoveAt(0);
                if (Baits.Count == 0)
                    Activation = default;
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Baits.Select(b => new AOEInstance(new AOEShapeCircle(3), b, default, Activation));
}

class LightningBolt(BossModule module) : Components.StandardAOEs(module, AID.LightningBolt, 4);

// strikes about every 10.5s
class TempestPiece(BossModule module) : Components.GenericAOEs(module)
{
    public DateTime NextActivation { get; private set; }
    public WPos? NextPosition { get; private set; }

    private Actor? Cloud;

    public override void Update()
    {
        if (Cloud == null || NextPosition != null)
            return;

        var move = Angle.FromDirection(Cloud.LastFrameMovement);
        if (move == default)
            return;

        if (move.AlmostEqual(120.Degrees(), 60.Degrees().Rad))
            NextPosition = new(115, 92);

        if (move.AlmostEqual(-120.Degrees(), 60.Degrees().Rad))
            NextPosition = new(87, 92);

        if (move.AlmostEqual(0.Degrees(), 60.Degrees().Rad))
            NextPosition = new(100, 115);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.TempestPiece)
        {
            Cloud = actor;
            NextPosition = actor.Position;
            NextActivation = WorldState.FutureTime(6.9f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Highlightning)
        {
            NumCasts++;
            NextPosition = null;
            NextActivation = WorldState.FutureTime(10.5f);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(NextPosition).Select(p => new AOEInstance(new AOEShapeCircle(21), p, default, NextActivation));
}

class Highlightning(BossModule module) : Components.CastCounter(module, AID.Highlightning);

class LightningStorm(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(8), (uint)IconID.LightningStorm, AID.LightningStorm, 15, true)
{
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        foreach (var (island, shape) in RM06SSugarRiot.IslandCones)
            if (ActiveBaitsNotOn(pc).Any(b => !IsOnBridge(b.Target) && b.Target.Position.InCone(Arena.Center, island, 60.Degrees())))
                Arena.ZoneComplex(Arena.Center, default, shape, ArenaColor.AOE);
    }

    // one player could technically hit both safe islands with the same spread, but that seems pretty unlikely and would also be a huge PITA to calculate (circle + concave poly intersection)
    // don't bother adding hint specifically for standing on the grass here, standard spread hint covers that
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (CurrentBaits.Count > 0 && !ActiveBaitsOn(actor).Any() && !IsOnBridge(actor))
            hints.Add("GTFO from grass!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (CurrentBaits.Count > 0 && !ActiveBaitsOn(actor).Any())
            hints.AddForbiddenZone(p => !IsOnBridge(p), CurrentBaits[0].Activation);
    }

    private static bool IsOnBridge(Actor p) => IsOnBridge(p.Position);
    private static bool IsOnBridge(WPos p) => RM06SSugarRiot.Bridges.Any(b => RM06SSugarRiot.BridgeShape.Check(p, b.Center, b.Rotation));
}
