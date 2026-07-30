namespace BossMod.Dawntrail.Ultimate.DMU;

// Used for displaying the casts after they have locked in
sealed class AllThingsEndingCasts(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AllThingsEnding, (uint)AID.AllThingsEnding1], new AOEShapeCone(100f, 90f.Degrees()));

// Used for displaying where the baits for the casts currently are before they lock in
// Baits are guessed by closest player to the clone
sealed class AllThingsEnding(BossModule module) : Components.GenericBaitAway(module, onlyShowOutlines: true)
{
    private enum BaitType { None, Far, Close }
    private BaitType currentBait = BaitType.None;
    private readonly List<Actor> clones = [with(4)]; // Also includes the boss since he will cast the same spell
    private readonly List<Actor> baiters = [with(4)]; // players currently baiting
    private WPos? lastKnownTowerMidPoint = null;
    public bool aoesLocked = true; // Used to prevent the aoes baits constantly showing
    private readonly AOEShapeCone cone = new(25f, 90f.Degrees());

    private readonly PathOfLight? towers = module.FindComponent<PathOfLight>();
    private readonly ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private static readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FuturesEndCast:
                currentBait = BaitType.Far;
                break;
            case (uint)AID.PastsEndCast:
                currentBait = BaitType.Close;
                break;
            case (uint)AID.AllThingsEnding:
            case (uint)AID.AllThingsEnding1:
                aoesLocked = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PastsEndSpread or (uint)AID.PastsEndSpread1 or (uint)AID.FuturesEndSpread or (uint)AID.FuturesEndSpread1)
        {
            clones.Add(caster);
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        baiters.Clear();

        if (aoesLocked)
        {
            return;
        }

        foreach (var clone in clones)
        {
            var baiter = Raid.WithoutSlot().Where(p => !baiters.Contains(p)).SortedByRange(clone.Position).Take(1).FirstOrDefault();
            if (baiter == null)
            {
                continue;
            }

            baiters.Add(baiter);
            var direction = clone.AngleTo(baiter);
            if (currentBait == BaitType.Close)
            {
                direction += 180.Degrees();
            }
            CurrentBaits.Add(new(clone.Position, baiter, cone, customRotation: direction));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (aoesLocked)
        {
            return;
        }

        if (towers == null || shapes == null)
        {
            return;
        }

        if (shapes.currentTowerSet % 2 == 0 && shapes.currentTowerSet != 8)
        {
            return;
        }

        // Final set displays safe spots differently compared to every other tower set
        if (shapes.currentTowerSet == 9)
        {
            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                var waymark = WorldState.Waymarks.GetFieldMark((int)Waymark.A);
                if (waymark == null)
                {
                    return;
                }

                Arena.ZoneCircleOutline(waymark.Value.ToWPos(), 1.0f, Colors.Safe, 2.0f);
                return;
            }

            if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                if (lastKnownTowerMidPoint != null)
                {
                    BaitSafeSpot(lastKnownTowerMidPoint.Value);
                }
            }

            return;
        }

        if (towers.Towers.Count != 2 || towers.CurrentSE == null || towers.CurrentSW == null)
        {
            return;
        }

        var midpoint = new WPos((towers.CurrentSW.Value.Position.X + towers.CurrentSE.Value.Position.X) * 0.5f,
                                (towers.CurrentSW.Value.Position.Z + towers.CurrentSE.Value.Position.Z) * 0.5f);
        lastKnownTowerMidPoint = midpoint;

        if (shapes.currentTowerSet == 8)
        { // Just to collect the lastKnownTowerMidPoint otherwise it will be null
            return;
        }

        BaitSafeSpot(midpoint);
    }

    private void BaitSafeSpot(WPos midPoint)
    {
        var newSouth = (midPoint - Arena.Center).Normalized();
        if (currentBait == BaitType.Close)
        {
            Arena.ZoneCircleOutline(midPoint + newSouth * 2.0f, 1.0f, Colors.Safe, 2.0f);
        }
        else if (currentBait == BaitType.Far)
        {
            Arena.ZoneCircleOutline(midPoint - newSouth * 13.0f, 1.0f, Colors.Safe, 2.0f);
        }
    }
}
