namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class UltimateEmbrace(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.UltimateEmbrace, 5.0f);

sealed class Forsaken(BossModule module) : Components.RaidwideCast(module, (uint)AID.Forsaken);

sealed class LightOfJudgmentP2(BossModule module) : Components.RaidwideCast(module, (uint)AID.LightOfJudgmentP2);

// Used for towers' spawn locations and marking them as SW or SE depending on the spawn point.
sealed class PathOfLight(BossModule module) : Components.GenericTowers(module, (uint)AID.ThePathOfLight)
{
    public Tower? CurrentSW;
    public Tower? CurrentSE;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index >= 1 && index <= 8 && state == 131073)
        {
            var angle = (180f - (index - 1) * 45f).Degrees();
            Towers.Add(new(Arena.Center + angle.ToDirection() * 8, 4, 2, 2, default, WorldState.FutureTime(10d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Towers.RemoveAll(tower => tower.Position.AlmostEqual(caster.Position, 1.0f));
        }
    }

    public override void Update()
    {
        if (Towers.Count != 2)
        {
            return;
        }

        var tower1 = Towers[0].Position;
        var tower2 = Towers[1].Position;

        var middleOfTowers = new WPos((tower1.X + tower2.X) * 0.5f, (tower1.Z + tower2.Z) * 0.5f);
        var southDirection = (middleOfTowers - Arena.Center).Normalized();
        if (southDirection.Length() <= 0)
        {
            return;
        }

        if ((tower1 - middleOfTowers).Dot(southDirection.OrthoL()) <= (tower2 - middleOfTowers).Dot(southDirection.OrthoL()))
        {
            CurrentSW = Towers[0];
            CurrentSE = Towers[1];
        }
        else
        {
            CurrentSW = Towers[1];
            CurrentSE = Towers[0];
        }
    }
}

// Used for setting up each player's role, such as the shape the player has, the pair the player belongs, if the player is a helper or soaker, etc.
sealed class ForsakenShapes(BossModule module) : BossComponent(module)
{
    private static readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private static readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    public int currentTowerSet = 1; // We start on odd tower set
    public bool towerSetLocked = false;
    public DateTime? lastTowerSetChange = null;

    public enum Shape { None, Spread, Cone, Stack }
    public Shape[] shapes = new Shape[8];
    public enum TowerRole { Unknown, Helper, Taker }

    public BitMask swSoakers;
    public BitMask seSoakers;
    public BitMask supportHelpers;
    public BitMask dpsHelpers;

    // TODO merge these together
    public bool pairsLocked = false;
    private bool pairsSwapped = false;

    public sealed class PairInfo(PartyRolesConfig.Assignment player1, PartyRolesConfig.Assignment player2, bool isSupport)
    {
        public PartyRolesConfig.Assignment player1Assignment = player1;
        public PartyRolesConfig.Assignment player2Assignment = player2;
        public bool isSupport = isSupport;
        public TowerRole role = TowerRole.Unknown;
    }

    public readonly PairInfo[] pairs = [
        new(PartyRolesConfig.Assignment.MT, PartyRolesConfig.Assignment.H1, true),
        new(PartyRolesConfig.Assignment.OT, PartyRolesConfig.Assignment.H2, true),
        new(PartyRolesConfig.Assignment.M1, PartyRolesConfig.Assignment.R1, false),
        new(PartyRolesConfig.Assignment.M2, PartyRolesConfig.Assignment.R2, false),
    ];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ThePathOfLight)
        {
            if (!towerSetLocked)
            {
                lastTowerSetChange = WorldState.CurrentTime;
                towerSetLocked = true;
                currentTowerSet++;
            }

            if (currentTowerSet is 4 or 8)
            {
                pairsSwapped = false;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var shape = iconID switch
        {
            (uint)IconID.TowerSpreadIcon => Shape.Spread,
            (uint)IconID.TowerConeIcon => Shape.Cone,
            (uint)IconID.TowerStackIcon => Shape.Stack,
            _ => default
        };

        if (shape != default)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                shapes[slot] = shape;
            }
        }
    }

    public override void Update()
    {
        swSoakers = default;
        seSoakers = default;
        supportHelpers = default;
        dpsHelpers = default;

        if (WorldState.CurrentTime - lastTowerSetChange > TimeSpan.FromSeconds(1.0) && towerSetLocked)
        {
            towerSetLocked = false;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }

        SetupPairs(slots);

        if ((currentTowerSet == 4 || currentTowerSet == 8) && !pairsSwapped)
        {
            foreach (var pair in pairs)
            {
                pair.role = pair.role switch
                {
                    TowerRole.Helper => TowerRole.Taker,
                    TowerRole.Taker => TowerRole.Helper,
                    _ => pair.role
                };
            }

            pairsSwapped = true;
        }

        foreach (var pair in pairs)
        {
            var slotPlayer1 = slots[(int)pair.player1Assignment];
            var slotPlayer2 = slots[(int)pair.player2Assignment];
            var shapeA = shapes[slotPlayer1];
            var shapeB = shapes[slotPlayer2];

            if (pair.role == TowerRole.Helper)
            {
                if (pair.isSupport)
                {
                    supportHelpers.Set(slotPlayer1);
                    supportHelpers.Set(slotPlayer2);
                }
                else
                {
                    dpsHelpers.Set(slotPlayer1);
                    dpsHelpers.Set(slotPlayer2);
                }
            }

            if (pair.role == TowerRole.Taker && (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_Markerless ||
                                                 dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers))
            {
                // First set of towers (tower set odd)
                if (currentTowerSet % 2 != 0)
                {
                    // Case: for the first set of towers no adjustment is needed between the Melee & Tank
                    if (currentTowerSet == 1)
                    {
                        if (shapeA == Shape.Cone || shapeB == Shape.Cone)
                        {
                            swSoakers.Set(slotPlayer1);
                            swSoakers.Set(slotPlayer2);
                        }

                        if (shapeA == Shape.Spread || shapeB == Shape.Spread)
                        {
                            seSoakers.Set(slotPlayer1);
                            seSoakers.Set(slotPlayer2);
                        }
                    }

                    // Case: every odd tower beyond the first set, the Melee & Tank may need to adjust base on pair shapes
                    if (currentTowerSet > 1)
                    {
                        // Cones and spreads are forced, where cone is always SW and spread is always SE
                        if (shapeA == Shape.Cone)
                        {
                            swSoakers.Set(slotPlayer1);
                        }

                        if (shapeB == Shape.Cone)
                        {
                            swSoakers.Set(slotPlayer2);
                        }

                        if (shapeA == Shape.Spread)
                        {
                            seSoakers.Set(slotPlayer1);
                        }

                        if (shapeB == Shape.Spread)
                        {
                            seSoakers.Set(slotPlayer2);
                        }

                        // If the pairs has the same shape, an adjustment is needed
                        if (shapeA == shapeB)
                        {
                            // If supports are the same shape, MT/OT has to go to the SE tower
                            if (pair.isSupport)
                            {
                                seSoakers.Set(slotPlayer1);
                                swSoakers.Set(slotPlayer2);
                            }

                            // If dps are the same shape, M1/M2 goes to the SW tower
                            if (!pair.isSupport)
                            {
                                swSoakers.Set(slotPlayer1);
                                seSoakers.Set(slotPlayer2);
                            }
                        }
                        else
                        { // Otherwise people just go to their default side
                            if (pair.isSupport)
                            {
                                if (shapeA == Shape.Stack)
                                {
                                    swSoakers.Set(slotPlayer1);
                                }

                                if (shapeB == Shape.Stack)
                                {
                                    swSoakers.Set(slotPlayer2);
                                }
                            }

                            if (!pair.isSupport)
                            {
                                if (shapeA == Shape.Stack)
                                {
                                    seSoakers.Set(slotPlayer1);
                                }

                                if (shapeB == Shape.Stack)
                                {
                                    seSoakers.Set(slotPlayer2);
                                }
                            }
                        }
                    }
                }

                // Second set of towers (tower set even)
                if (currentTowerSet % 2 == 0)
                {
                    if (pair.isSupport)
                    {
                        // They have different shapes - both go to the same tower which is west tower
                        if (shapeA != shapeB)
                        {
                            swSoakers.Set(slotPlayer1);
                            swSoakers.Set(slotPlayer2);
                        }
                        else
                        { // healer goes to SW tower, tank goes to SE tower - player2 is healer, player1 is tank
                            swSoakers.Set(slotPlayer2);
                            seSoakers.Set(slotPlayer1);
                        }
                    }

                    if (!pair.isSupport)
                    {
                        if (shapeA != shapeB)
                        {
                            // They have different shapes - both go to the same tower which is east tower
                            seSoakers.Set(slotPlayer1);
                            seSoakers.Set(slotPlayer2);
                        }
                        else
                        {
                            // range goes to SE tower, melee goes to SE tower - player2 is range, player1 is melee
                            seSoakers.Set(slotPlayer2);
                            swSoakers.Set(slotPlayer1);
                        }
                    }
                }
            }

            if (pair.role == TowerRole.Taker && dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                // First set of towers (tower set odd)
                if (currentTowerSet % 2 != 0)
                {
                    // Cones and spreads are forced, where cone is always SW and spread is always SE
                    if (shapeA == Shape.Cone)
                    {
                        swSoakers.Set(slotPlayer1);
                    }

                    if (shapeB == Shape.Cone)
                    {
                        swSoakers.Set(slotPlayer2);
                    }

                    if (shapeA == Shape.Spread)
                    {
                        seSoakers.Set(slotPlayer1);
                    }

                    if (shapeB == Shape.Spread)
                    {
                        seSoakers.Set(slotPlayer2);
                    }

                    // If the pairs have the same shape, an adjustment is needed
                    if (shapeA == shapeB)
                    {
                        // If supports are the same shape, MT/OT has to go to the SE tower
                        if (pair.isSupport)
                        {
                            seSoakers.Set(slotPlayer1);
                            swSoakers.Set(slotPlayer2);
                        }

                        // If dps are the same shape, M1/M2 goes to the SW tower
                        if (!pair.isSupport)
                        {
                            swSoakers.Set(slotPlayer1);
                            seSoakers.Set(slotPlayer2);
                        }
                    }
                    else
                    {
                        if (pair.isSupport)
                        {
                            if (shapeA == Shape.Stack)
                            {
                                swSoakers.Set(slotPlayer1);
                            }

                            if (shapeB == Shape.Stack)
                            {
                                swSoakers.Set(slotPlayer2);
                            }
                        }

                        if (!pair.isSupport)
                        {
                            if (shapeA == Shape.Stack)
                            {
                                seSoakers.Set(slotPlayer1);
                            }

                            if (shapeB == Shape.Stack)
                            {
                                seSoakers.Set(slotPlayer2);
                            }
                        }
                    }
                }

                // Second set of towers (tower set even)
                if (currentTowerSet % 2 == 0)
                {
                    if (pair.isSupport)
                    {
                        // They have different shapes - both go to the same tower which is west tower
                        if (shapeA != shapeB)
                        {
                            swSoakers.Set(slotPlayer1);
                            swSoakers.Set(slotPlayer2);
                        }
                        else
                        { // healer goes to SW tower, tank goes to SE tower - player2 is healer, player1 is tank
                            swSoakers.Set(slotPlayer2);
                            seSoakers.Set(slotPlayer1);
                        }
                    }

                    if (!pair.isSupport)
                    {
                        if (shapeA != shapeB)
                        {
                            // They have different shapes - both go to the same tower which is east tower
                            seSoakers.Set(slotPlayer1);
                            seSoakers.Set(slotPlayer2);
                        }
                        else
                        {
                            // range goes to SE tower, melee goes to SW tower - player2 is range, player1 is melee
                            seSoakers.Set(slotPlayer2);
                            swSoakers.Set(slotPlayer1);
                        }
                    }
                }
            }
        }
    }

    private void SetupPairs(int[] slots)
    {
        if (pairsLocked)
        {
            return;
        }

        pairsLocked = true;

        foreach (var pair in pairs)
        {
            if (pair.role != TowerRole.Unknown)
            {
                continue;
            }

            var shapeA = shapes[slots[(int)pair.player1Assignment]];
            var shapeB = shapes[slots[(int)pair.player2Assignment]];

            if (shapeA == Shape.None || shapeB == Shape.None)
            {
                pairsLocked = false;
                continue;
            }

            pair.role = shapeA == shapeB ? TowerRole.Helper : TowerRole.Taker;
        }

        foreach (var pair in pairs)
        {
            if (pair.role == TowerRole.Unknown)
            {
                pairsLocked = false;
                break;
            }
        }
    }
}

sealed class ForsakenBaitsSpreadStacks(BossModule module) : Components.UniformStackSpread(module, 5, 5, 3)
{
    private readonly ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private readonly PathOfLight? towers = module.FindComponent<PathOfLight>();

    public override void Update()
    {
        if (towers == null || shapes == null)
        {
            return;
        }

        Stacks.Clear();
        Spreads.Clear();

        foreach (var (i, player) in Raid.WithSlot())
        {
            if (towers.Towers.Any(t => player.Position.InCircle(t.Position, 4.00f)))
            {
                if (shapes.shapes[i] == ForsakenShapes.Shape.Stack)
                {
                    AddStack(player);
                }

                if (shapes.shapes[i] == ForsakenShapes.Shape.Spread)
                {
                    AddSpread(player);
                }
            }
        }
    }
}

sealed class ForsakenBaitsCone(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Spellwave)
{
    private readonly ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private readonly PathOfLight? towers = module.FindComponent<PathOfLight>();

    public override void Update()
    {
        if (towers == null || shapes == null)
        {
            return;
        }

        CurrentBaits.Clear();

        foreach (var (i, player) in Raid.WithSlot())
        {
            if (towers.Towers.Any(t => player.Position.InCircle(t.Position, 4.00f)))
            {
                if (shapes.shapes[i] == ForsakenShapes.Shape.Cone)
                {
                    var closestPlayer = Raid.WithoutSlot().Exclude(player).Closest(player.Position);
                    if (closestPlayer != null)
                    {
                        CurrentBaits.Add(new(player, closestPlayer, new AOEShapeCone(40, 45.Degrees())));
                    }
                }
            }
        }
    }
}

sealed class ForsakenBaitsBossClones(BossModule module) : Components.UniformStackSpread(module, 5, 5)
{
    private readonly List<Actor> clones = []; // Also includes the boss since he will cast the same spell
    private readonly List<Actor> baiters = []; // List of players currently baiting - prevents dupes
    private int NumCasts = 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.FuturesEndCast) or ((uint)AID.PastsEndCast))
        {
            foreach (var actor in WorldState.Actors)
            {
                if (actor.OID is ((uint)OID.BossP2) or ((uint)OID.P2KefkaHelpers))
                {
                    clones.Add(actor);
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.PastsEndSpread) or ((uint)AID.PastsEndSpread1) or
            ((uint)AID.FuturesEndSpread) or ((uint)AID.FuturesEndSpread1))
        {
            NumCasts++;

            if (NumCasts == 4)
            {
                clones.Clear();
                NumCasts = 0;
            }
        }
    }

    public override void Update()
    {
        Spreads.Clear();
        baiters.Clear();

        if (clones.Count == 0)
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
            AddSpread(baiter);
        }
    }
}

// Used for odd tower sets in figuring out what each player is responsible for
sealed class ForsakenSolverSet1(BossModule module) : BossComponent(module)
{
    private readonly ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private readonly PathOfLight? towers = module.FindComponent<PathOfLight>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();
    public uint colourCircle = Colors.Safe;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (shapes == null || towers == null)
        {
            return;
        }

        if (towers.Towers.Count != 2 || shapes.swSoakers.None() || shapes.seSoakers.None() || towers.CurrentSW == null || towers.CurrentSE == null)
        {
            return;
        }

        if (shapes.currentTowerSet % 2 == 0)
        {
            return;
        }

        var midpoint = new WPos((towers.CurrentSW.Value.Position.X + towers.CurrentSE.Value.Position.X) * 0.5f,
            (towers.CurrentSW.Value.Position.Z + towers.CurrentSE.Value.Position.Z) * 0.5f);
        var newSouth = (midpoint - Arena.Center).Normalized();

        var towardSW = (towers.CurrentSW.Value.Position - midpoint).Normalized();
        var towardSE = (towers.CurrentSE.Value.Position - midpoint).Normalized();

        // Case: SW players with different debuffs
        if (shapes.swSoakers[pcSlot])
        {
            var swPosition = towers.CurrentSW.Value.Position;
            var shape = shapes.shapes[pcSlot];

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or
                DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (shape == ForsakenShapes.Shape.Stack)
                {
                    Arena.ZoneCircleOutline(swPosition + (-towardSW * 0.5f) + (-newSouth * 1.0f), 1.0f, colourCircle, 2.0f);
                }

                if (shape == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(swPosition + (newSouth * 3.0f), 1.0f, colourCircle, 2.0f);
                }
            }

            if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var toCenter = (Arena.Center - swPosition).Normalized();
                if (toCenter.LengthSq() <= 0)
                {
                    return;
                }

                if (shape == ForsakenShapes.Shape.Stack)
                {
                    Arena.ZoneCircleOutline(swPosition + 1.0f * toCenter + 0.5f * toCenter.Rotate(90f.Degrees()), 1.0f, colourCircle, 2.0f);
                }

                if (shape == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(swPosition - 3.0f * toCenter, 1.0f, colourCircle, 2.0f);
                }
            }
        }

        // Case: SW players with same debuffs
        if (shapes.supportHelpers[pcSlot])
        {
            var swPosition = towers.CurrentSW.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or
                DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (assignment is PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2)
                {
                    Arena.ZoneCircleOutline(swPosition + (newSouth * 4.5f), 1.0f, colourCircle, 2.0f);
                }

                if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
                {
                    Arena.ZoneCircleOutline(swPosition + (-towardSW * 3.0f) + (-newSouth * 4.0f), 1.0f, colourCircle, 2.0f);
                }
            }

            if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var toCenter = (Arena.Center - swPosition).Normalized();
                if (toCenter.LengthSq() <= 0)
                {
                    return;
                }

                if (assignment is PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2)
                {
                    Arena.ZoneCircleOutline(swPosition - 4.5f * toCenter, 1.0f, colourCircle, 2.0f);
                }

                if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
                {
                    Arena.ZoneCircleOutline(swPosition + 4.5f * toCenter + 0.5f * toCenter.Rotate(90f.Degrees()), 1.0f, colourCircle, 2.0f);
                }
            }
        }

        // Case: SE players with different debuffs
        if (shapes.seSoakers[pcSlot])
        {
            var sePosition = towers.CurrentSE.Value.Position;
            var shape = shapes.shapes[pcSlot];

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or
                DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (shape == ForsakenShapes.Shape.Stack)
                {
                    Arena.ZoneCircleOutline(sePosition + (-towardSE * 2.5f) + (newSouth * 2.5f), 1.0f, colourCircle, 2.0f);
                }

                if (shape == ForsakenShapes.Shape.Spread)
                {
                    Arena.ZoneCircleOutline(sePosition + (towardSE * 2.0f) + (-newSouth * 3.0f), 1.0f, colourCircle, 2.0f);
                }
            }

            if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var toCenter = (Arena.Center - sePosition).Normalized();
                if (toCenter.LengthSq() <= 0)
                {
                    return;
                }

                if (shape == ForsakenShapes.Shape.Stack)
                {
                    Arena.ZoneCircleOutline(sePosition + 3.0f * toCenter - 2.0f * toCenter.Rotate(90f.Degrees()), 1.0f, colourCircle, 2.0f);
                }

                if (shape == ForsakenShapes.Shape.Spread)
                {
                    Arena.ZoneCircleOutline(sePosition - 2.5f * toCenter + 2.5f * toCenter.Rotate(90f.Degrees()), 1.0f, colourCircle, 2.0f);
                }
            }
        }

        // Case: SE players with same debuffs
        if (shapes.dpsHelpers[pcSlot])
        {
            var sePosition = towers.CurrentSE.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or
                DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2)
                {
                    Arena.ZoneCircleOutline(sePosition + (-towardSE * 4.0f) + (newSouth * 3.0f), 1.0f, colourCircle, 2.0f);
                }

                if (assignment is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
                {
                    Arena.ZoneCircleOutline(sePosition + (-towardSE * 4.0f) + (newSouth * 3.0f), 1.0f, colourCircle, 2.0f);
                }
            }

            if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                var toCenter = (Arena.Center - sePosition).Normalized();
                if (toCenter.LengthSq() <= 0)
                {
                    return;
                }

                if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2)
                {
                    Arena.ZoneCircleOutline(sePosition + 4.5f * toCenter - 1.0f * toCenter.Rotate(90f.Degrees()), 1.0f, colourCircle, 2.0f);
                }

                if (assignment is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
                {
                    Arena.ZoneCircleOutline(sePosition + 4.5f * toCenter - 1.0f * toCenter.Rotate(90f.Degrees()), 1.0f, colourCircle, 2.0f);
                }
            }
        }
    }

    public override void Update()
    {
        if (shapes == null || towers == null)
        {
            return;
        }

        if (towers.Towers.Count != 2 || towers.CurrentSE == null || towers.CurrentSW == null)
        {
            return;
        }

        if (shapes.currentTowerSet % 2 == 0)
        {
            return;
        }

        var party = new BitMask(0xFF);

        for (int i = 0; i < towers.Towers.Count; i++)
        {
            var t = towers.Towers[i];

            if (t.Position.AlmostEqual(towers.CurrentSW.Value.Position, 1.0f))
            {
                t.ForbiddenSoakers = party & ~shapes.swSoakers;
            }

            if (t.Position.AlmostEqual(towers.CurrentSE.Value.Position, 1.0f))
            {
                t.ForbiddenSoakers = party & ~shapes.seSoakers;
            }

            towers.Towers[i] = t;
        }
    }
}

// Used for even tower sets in figuring out what each player is responsible for
sealed class ForsakenSolverSet2(BossModule module) : BossComponent(module)
{
    private readonly ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private readonly PathOfLight? towers = module.FindComponent<PathOfLight>();
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (shapes == null || towers == null)
        {
            return;
        }

        if (towers.Towers.Count != 2 || shapes.swSoakers.None() || shapes.seSoakers.None() || towers.CurrentSW == null || towers.CurrentSE == null)
        {
            return;
        }

        if (shapes.currentTowerSet % 2 != 0)
        {
            return;
        }

        // Case: SW players with different debuffs (soakers)
        if (shapes.swSoakers[pcSlot])
        {
            var swPosition = towers.CurrentSW.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            var toCenter = (Arena.Center - swPosition).Normalized();
            if (toCenter.LengthSq() <= 0)
            {
                return;
            }

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or
                DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(swPosition + toCenter.Normalized() * 3.5f, 0.75f, Colors.Safe, 1.0f);
                }

                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Spread)
                {
                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_Markerless)
                    {
                        Arena.ZoneCircleOutline(swPosition + (-toCenter.Normalized()) * 3.5f, 0.75f, Colors.Safe, 1.0f);
                    }

                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
                    {
                        Arena.ZoneCircleOutline(swPosition + (-toCenter).Rotate(34f.Degrees()).Normalized() * 3.57f, 0.75f, Colors.Safe, 1.0f);
                    }
                }
            }

            if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(swPosition + 3.0f * toCenter + 2.0f * toCenter.Rotate(90f.Degrees()), 0.75f, Colors.Safe, 1.0f);
                }

                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Spread)
                {
                    Arena.ZoneCircleOutline(swPosition - 3.0f * toCenter - 2.0f * toCenter.Rotate(90f.Degrees()), 0.75f, Colors.Safe, 1.0f);
                }
            }
        }

        // Case: SW players with same debuffs (helpers)
        if (shapes.supportHelpers[pcSlot])
        {
            var swPosition = towers.CurrentSW.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            var toCenter = (Arena.Center - swPosition).Normalized();
            if (toCenter.LengthSq() <= 0)
            {
                return;
            }

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or
                DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (assignment is PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2)
                {
                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_Markerless)
                    {
                        Arena.ZoneCircleOutline(swPosition + toCenter.Rotate(90f.Degrees()).Normalized() * 4.5f, 0.75f, Colors.Safe, 1.0f);
                    }

                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
                    {
                        Arena.ZoneCircleOutline(swPosition + toCenter.Rotate(82f.Degrees()).Normalized() * 7.07f, 0.75f, Colors.Safe, 1.0f);
                    }
                }

                if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
                {
                    Arena.ZoneCircleOutline(swPosition + toCenter.Rotate(35.0f.Degrees()).Normalized() * 11.5f, 0.75f, Colors.Safe, 1.0f);
                }
            }

            if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                if (assignment is PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2)
                {
                    Arena.ZoneCircleOutline(swPosition + 1.0f * toCenter + 7.0f * toCenter.Rotate(90f.Degrees()), 0.75f, Colors.Safe, 1.0f);
                }

                if (assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT)
                {
                    Arena.ZoneCircleOutline(swPosition + 9.0f * toCenter + 6.0f * toCenter.Rotate(90f.Degrees()), 0.75f, Colors.Safe, 1.0f);
                }
            }
        }

        // Case: SE players with different debuffs (soakers)
        if (shapes.seSoakers[pcSlot])
        {
            var sePosition = towers.CurrentSE.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            var toCenter = (Arena.Center - sePosition).Normalized();
            if (toCenter.LengthSq() <= 0)
            {
                return;
            }

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or
                DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(sePosition + toCenter.Normalized() * 3.5f, 0.75f, Colors.Safe, 1.0f);
                }

                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Spread)
                {
                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_Markerless)
                    {
                        Arena.ZoneCircleOutline(sePosition + (-toCenter.Normalized()) * 3.5f, 0.75f, Colors.Safe, 1.0f);
                    }

                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
                    {
                        Arena.ZoneCircleOutline(sePosition + (-toCenter).Rotate(-26f.Degrees()).Normalized() * 3.6f, 0.75f, Colors.Safe, 1.0f);
                    }
                }
            }

            if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Cone)
                {
                    Arena.ZoneCircleOutline(sePosition + 3.0f * toCenter - 2.0f * toCenter.Rotate(90f.Degrees()), 0.75f, Colors.Safe, 1.0f);
                }

                if (shapes.shapes[pcSlot] == ForsakenShapes.Shape.Spread)
                {
                    Arena.ZoneCircleOutline(sePosition - 3.0f * toCenter + 2.0f * toCenter.Rotate(90f.Degrees()), 0.75f, Colors.Safe, 1.0f);
                }
            }
        }

        // Case: SE players with same debuffs (helpers)
        if (shapes.dpsHelpers[pcSlot])
        {
            var sePosition = towers.CurrentSE.Value.Position;
            var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

            var toCenter = (Arena.Center - sePosition).Normalized();
            if (toCenter.LengthSq() <= 0)
            {
                return;
            }

            if (dmuConfig.P2Forsaken is DMUConfig.P2ForsakenStrategy.Meow_Markerless or
                DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
            {
                if (assignment is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
                {
                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_Markerless)
                    {
                        Arena.ZoneCircleOutline(sePosition + toCenter.Rotate(-90f.Degrees()).Normalized() * 4.5f, 0.75f, Colors.Safe, 1.0f);
                    }

                    if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers)
                    {
                        Arena.ZoneCircleOutline(sePosition + toCenter.Rotate(-82f.Degrees()).Normalized() * 7.07f, 0.75f, Colors.Safe, 1.0f);
                    }
                }

                if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2)
                {
                    Arena.ZoneCircleOutline(sePosition + toCenter.Rotate(-35.0f.Degrees()).Normalized() * 11.5f, 0.75f, Colors.Safe, 1.0f);
                }
            }

            if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex)
            {
                if (assignment is PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2)
                {
                    Arena.ZoneCircleOutline(sePosition + 1.0f * toCenter - 7.0f * toCenter.Rotate(90f.Degrees()), 0.75f, Colors.Safe, 1.0f);
                }

                if (assignment is PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2)
                {
                    Arena.ZoneCircleOutline(sePosition + 9.0f * toCenter - 6.0f * toCenter.Rotate(90f.Degrees()), 0.75f, Colors.Safe, 1.0f);
                }
            }
        }
    }

    public override void Update()
    {
        if (shapes == null || towers == null)
        {
            return;
        }

        if (towers.Towers.Count != 2 || towers.CurrentSE == null || towers.CurrentSW == null)
        {
            return;
        }

        if (shapes.currentTowerSet % 2 != 0)
        {
            return;
        }

        var party = new BitMask(0xFF);

        for (int i = 0; i < towers.Towers.Count; i++)
        {
            var t = towers.Towers[i];

            if (t.Position.AlmostEqual(towers.CurrentSW.Value.Position, 1.0f))
            {
                t.ForbiddenSoakers = party & ~shapes.swSoakers;
            }

            if (t.Position.AlmostEqual(towers.CurrentSE.Value.Position, 1.0f))
            {
                t.ForbiddenSoakers = party & ~shapes.seSoakers;
            }

            towers.Towers[i] = t;
        }
    }
}

sealed class WingsOfDestructionLeftRight(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.WingsOfDestructionLeft, (uint)AID.WingsOfDestructionRight], new AOEShapeRect(80, 20));

sealed class WingsOfDestructionTB(BossModule module) : Components.GenericBaitAway(module, (uint)AID.WingsOfDestructionTB, true, true)
{
    private Actor? casterPosition;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WingsOfDestructionTB)
        {
            casterPosition = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WingsOfDestructionTB1)
        {
            casterPosition = null;
            NumCasts++;
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (casterPosition == null)
        {
            return;
        }

        var players = Raid.WithoutSlot().SortedByRange(casterPosition.Position).ToList();
        if (players.Count > 1)
        {
            CurrentBaits.Add(new(casterPosition, players[0], new AOEShapeCircle(7f)));
            CurrentBaits.Add(new(casterPosition, players[^1], new AOEShapeCircle(7f)));
        }
    }
}

sealed class Trine(BossModule module) : Components.GenericAOEs(module, (uint)AID.Trine)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly List<Actor> triangles = [];
    private readonly float radius = 10 * MathF.Sqrt(3) / 3;
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is ((uint)OID.YellowTriangle) or ((uint)OID.YellowTriangle1))
        {
            triangles.Add(actor);
        }

        if (actor.OID == (uint)OID.YellowTriangle)
        {
            aoes.Add(new(new AOEShapeCircle(6f), actor.Position + new WDir(radius, 0f)));
            aoes.Add(new(new AOEShapeCircle(6f), actor.Position + new WDir(-radius / 2, 5f)));
            aoes.Add(new(new AOEShapeCircle(6f), actor.Position + new WDir(-radius / 2, -5f)));
        }

        if (actor.OID == (uint)OID.YellowTriangle1)
        {
            aoes.Add(new(new AOEShapeCircle(6f), actor.Position + new WDir(-radius, 0f)));
            aoes.Add(new(new AOEShapeCircle(6f), actor.Position + new WDir(radius / 2, 5f)));
            aoes.Add(new(new AOEShapeCircle(6f), actor.Position + new WDir(radius / 2, -5f)));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID is ((uint)OID.YellowTriangle) or ((uint)OID.YellowTriangle1))
        {
            if (state == (uint)Animations.TriangleExplosion)
            {
                aoes.RemoveRange(0, 3);
                NumCasts += 3;
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (aoes.Count == 0)
        {
            return CollectionsMarshal.AsSpan(aoes);
        }

        (int currentWave, int nextWave)[] wave = [(9, 3), (3, 9), (9, 0)];
        var (currentSize, nextSize) = wave[NumCasts < 9 ? 0 : NumCasts < 12 ? 1 : 2];
        var count = Math.Min(currentSize + nextSize, aoes.Count);
        for (int i = 0; i < count; i++)
        {
            aoes[i] = aoes[i] with
            {
                Color = i < currentSize ? Colors.Danger : Colors.AOE,
                Risky = i < currentSize
            };
        }

        return CollectionsMarshal.AsSpan(aoes)[..count];
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts < 9)
        {
            return;
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0)
        {
            return;
        }
        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        var waymarkA = WorldState.Waymarks.GetFieldMark((int)Waymark.A);
        var waymark1 = WorldState.Waymarks.GetFieldMark((int)Waymark.N1);

        if (waymarkA == null || waymark1 == null)
        {
            return;
        }

        var waymarkAAngle = (waymarkA.Value.ToWPos() - Arena.Center).ToAngle();
        var waymark1Angle = (waymark1.Value.ToWPos() - Arena.Center).ToAngle();
        var firstWave = triangles.Take(3).Select(t => t.Position).ToArray();

        Array.Sort(firstWave, delegate (WPos x, WPos y)
        {
            var xAngle = (x - Arena.Center).ToAngle();
            var yAngle = (y - Arena.Center).ToAngle();

            var xDeg = xAngle.AlmostEqual(waymarkAAngle, 0.01f) ? 180f : (xAngle - waymarkAAngle + 180f.Degrees()).Normalized().Deg;
            var yDeg = yAngle.AlmostEqual(waymarkAAngle, 0.01f) ? 180f : (yAngle - waymarkAAngle + 180f.Degrees()).Normalized().Deg;

            return xDeg < yDeg ? 1 : -1;
        });

        if (assignment is not PartyRolesConfig.Assignment.MT and not PartyRolesConfig.Assignment.OT)
        {
            Arena.ZoneCircleOutline(firstWave[0], 1f, Colors.Safe, 2f);
            return;
        }

        var boss = (Module as DMU)?.BossP2();
        if (boss == null)
        {
            return;
        }

        var ccwSpot = firstWave.MinBy(p => (waymark1Angle - (p - Arena.Center).ToAngle()).Normalized().Deg)!;
        if (assignment == PartyRolesConfig.Assignment.OT)
        {
            Arena.ZoneCircleOutline(Arena.Center + (ccwSpot - Arena.Center).Normalized() * Arena.Bounds.Radius, 1f, Colors.Safe, 2f);
        }

        WPos closestSpot = ccwSpot;
        for (float r = 0.5f; r <= (ccwSpot - boss.Position).Length(); r += 0.5f)
        {
            List<WPos> spots = [];
            for (int degree = -60; degree <= 60; degree += 5)
            {
                var spot = boss.Position + r * ((ccwSpot - boss.Position).ToAngle() + degree.Degrees()).ToDirection();

                if (!aoes.Any(aoe => aoe.Check(spot)))
                {
                    spots.Add(spot);
                }
            }

            if (spots.Count > 0)
            {
                closestSpot = spots.MinBy(spot => ((spot - boss.Position).ToAngle() - (ccwSpot - boss.Position).ToAngle()).Abs().Deg)!;
                break;
            }
        }

        if (assignment == PartyRolesConfig.Assignment.MT)
        {
            Arena.ZoneCircleOutline(closestSpot, 1f, Colors.Safe, 2f);
        }
    }
}
