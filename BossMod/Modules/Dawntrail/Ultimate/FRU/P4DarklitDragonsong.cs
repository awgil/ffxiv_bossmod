namespace BossMod.Dawntrail.Ultimate.FRU;

// tethers & general assignments
class P4DarklitDragonsong(BossModule module) : BossComponent(module)
{
    public BitMask Stacks;
    public BitMask TowerSoakers;
    public BitMask AssignS;
    public BitMask AssignE;
    private readonly List<(Actor from, Actor to)> _tethers = [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        var southTower = TowerSoakers & AssignS;
        if (southTower.Any())
            hints.Add($"Water in {((southTower & Stacks).Any() ? "S" : "N")} tower");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _tethers)
            Arena.AddLine(t.from.Position, t.to.Position, ArenaColor.Safe); // TODO: min/max break distance
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellInWaitingDarkWater)
            Stacks.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.LightRampantChains)
        {
            TowerSoakers.Set(Raid.FindSlot(source.InstanceID));
            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
                _tethers.Add((source, target));
            if (_tethers.Count == 4)
                InitAssignments();
        }
    }

    private void InitAssignments()
    {
        Span<int> ccwOrderSlots = [-1, -1, -1, -1, -1, -1, -1, -1];
        int[] playerPrios = [-1, -1, -1, -1, -1, -1, -1, -1];
        foreach (var (slot, group) in Service.Config.Get<FRUConfig>().P4DarklitDragonsongAssignments.Resolve(Raid))
        {
            ccwOrderSlots[group] = slot;
            playerPrios[slot] = group;
        }
        if (ccwOrderSlots.Contains(-1))
            return; // assignments are not valid, bail

        // find the anchor (tethered player with lowest prio), players tethered to him on both sides will take S tower
        var anchorSlot = TowerSoakers.SetBits().MinBy(i => playerPrios[i]);
        var anchorPlayer = Raid[anchorSlot];
        foreach (var t in _tethers)
        {
            if (t.from == anchorPlayer)
                AssignS.Set(Raid.FindSlot(t.to.InstanceID));
            else if (t.to == anchorPlayer)
                AssignS.Set(Raid.FindSlot(t.from.InstanceID));
        }

        // remaining assignments (N/S for baits, E/W for everyone) in prio order
        int numAssignedSoakers = 0, numAssignedBaits = 0;
        foreach (var slot in ccwOrderSlots)
        {
            if (TowerSoakers[slot])
            {
                // first and last prio go E
                AssignE[slot] = numAssignedSoakers++ is 0 or 3;
            }
            else
            {
                // first and last go N, last two go E
                AssignS[slot] = numAssignedBaits is 1 or 2;
                AssignE[slot] = numAssignedBaits >= 2;
                ++numAssignedBaits;
            }
        }

        // finally, if both stacks are on the same N/S side, bait needs to swap with other bait on same E/W side
        if ((AssignS & Stacks).NumSetBits() != 1)
        {
            var flexStack = Stacks & ~TowerSoakers; // mask containing one set bit, corresponding to non-soaker stack - he will need to flex
            var flexE = AssignE[flexStack.LowestSetBit()];
            var flexMask = (AssignE ^ new BitMask(flexE ? 0u : 0xFF)) & ~TowerSoakers; // mask containing both flexers
            if (flexStack.NumSetBits() != 1 || flexMask.NumSetBits() != 2 || (flexStack & flexMask) != flexStack)
                ReportError("Some error with swap logic, investigate");
            AssignS ^= flexMask;
        }
    }
}

class P4DarklitDragonsongBrightHunger(BossModule module) : Components.GenericTowers(module, ActionID.MakeSpell(AID.BrightHunger))
{
    private int _numTethers;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.LightRampantChains && ++_numTethers == 4)
        {
            var assignments = Module.FindComponent<P4DarklitDragonsong>();
            if (assignments != null)
            {
                var allowedN = assignments.TowerSoakers & ~assignments.AssignS;
                var allowedS = assignments.TowerSoakers & assignments.AssignS;
                if (assignments.AssignS.None())
                    allowedN = allowedS = assignments.TowerSoakers; // no assignments, just mark both towers as good

                var towerOffset = new WDir(0, 8);
                Towers.Add(new(Module.Center - towerOffset, 4, 4, 4, new BitMask(0xFF) ^ allowedN, WorldState.FutureTime(10.4f)));
                Towers.Add(new(Module.Center + towerOffset, 4, 4, 4, new BitMask(0xFF) ^ allowedS, WorldState.FutureTime(10.4f)));
            }
        }
    }
}

class P4DarklitDragonsongPathOfLight(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.PathOfLightAOE))
{
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeCone _shape = new(60, 30.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null && ForbiddenPlayers.Any())
            foreach (var p in Raid.WithoutSlot().SortedByRange(_source.Position).Take(4))
                CurrentBaits.Add(new(_source, p, _shape, _activation));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;

        var baitIndex = CurrentBaits.FindIndex(b => b.Target == actor);
        if (ForbiddenPlayers[slot])
        {
            if (baitIndex >= 0)
                hints.Add("Stay farther away!");
        }
        else
        {
            if (baitIndex < 0)
                hints.Add("Stay closer to bait!");
            else if (PlayersClippedBy(CurrentBaits[baitIndex]).Any())
                hints.Add("Bait cone away from raid!");
        }

        if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
            hints.Add("GTFO from baited cone!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DarklitDragonsongUsurper)
        {
            _source = caster;
            _activation = Module.CastFinishAt(spell, 12);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.LightRampantChains)
            ForbiddenPlayers.Set(Raid.FindSlot(source.InstanceID));
    }
}

class P4DarklitDragonsongDarkWater(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4, includeDeadTargets: true)
{
    private readonly P4DarklitDragonsong? _assignments = module.FindComponent<P4DarklitDragonsong>();

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellInWaitingDarkWater)
        {
            BitMask forbidden = default;
            if (_assignments != null && _assignments.AssignS.Any())
            {
                var isSouth = _assignments.AssignS[Raid.FindSlot(actor.InstanceID)];
                forbidden = _assignments.AssignS ^ new BitMask(isSouth ? 0xFF : 0u);
            }
            AddStack(actor, status.ExpireAt, forbidden);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DarkWater)
            Stacks.Clear();
    }
}

class P4SomberDance(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(8);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source == null)
            return;
        var targets = Raid.WithoutSlot(excludeNPCs: true);
        var target = NumCasts == 0 ? targets.Farthest(_source.Position) : targets.Closest(_source.Position);
        if (target != null)
            CurrentBaits.Add(new(_source, target, _shape, _activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SomberDance)
        {
            ForbiddenPlayers = Raid.WithSlot(true).WhereActor(p => p.Role != Role.Tank).Mask();
            _source = caster;
            _activation = Module.CastFinishAt(spell, 0.4f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SomberDanceAOE1 or AID.SomberDanceAOE2)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(3.2f);
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(Raid.FindSlot(t.ID));
        }
    }
}
