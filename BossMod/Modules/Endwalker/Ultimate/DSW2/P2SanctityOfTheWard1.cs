namespace BossMod.Endwalker.Ultimate.DSW2;

class P2SanctityOfTheWard1Gaze : DragonsGaze
{
    public P2SanctityOfTheWard1Gaze() : base(OID.BossP2)
    {
        EnableHints = true;
    }
}

// sacred sever - distance-based shared damage on 1/2/1/2 markers
class P2SanctityOfTheWard1Sever : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }
    public Actor? Source { get; private set; }

    public P2SanctityOfTheWard1Sever() : base(6, 0, 4) { }

    public override void Init(BossModule module)
    {
        Source = module.Enemies(OID.SerZephirin).FirstOrDefault();
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);

        if (Stacks.Count == 2 && Source != null)
        {
            arena.Actor(Source, ArenaColor.Enemy, true);
            arena.AddLine(Source.Position, Stacks[NumCasts % 2].Target.Position, ArenaColor.Danger);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SacredSever && ++NumCasts >= 4)
            Stacks.Clear();
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        switch ((IconID)iconID)
        {
            case IconID.SacredSever1:
                Stacks.Insert(0, new(actor, StackRadius, MinStackSize, MaxStackSize));
                break;
            case IconID.SacredSever2:
                AddStack(actor);
                break;
        }
    }
}

// shining blade (charges that leave orbs) + flares (their explosions)
class P2SanctityOfTheWard1Flares : Components.GenericAOEs
{
    public class ChargeInfo
    {
        public Actor Source;
        public List<AOEInstance> ChargeAOEs = new();
        public List<WPos> Spheres = new();

        public ChargeInfo(Actor source)
        {
            Source = source;
        }
    }

    public List<ChargeInfo> Charges = new();
    public Angle ChargeAngle { get; private set; } // 0 if charges are not active or on failure, <0 if CW, >0 if CCW

    private static readonly float _chargeHalfWidth = 3;
    private static readonly AOEShapeCircle _brightflareShape = new(9);

    public P2SanctityOfTheWard1Flares() : base(ActionID.MakeSpell(AID.BrightFlare), "GTFO from charges and spheres!") { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        foreach (var c in Charges)
        {
            foreach (var aoe in c.ChargeAOEs)
                yield return aoe;
            foreach (var s in c.Spheres.Take(6))
                yield return new(_brightflareShape, s);
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.Actors(Charges.Where(c => c.ChargeAOEs.Count > 0).Select(c => c.Source), ArenaColor.Enemy, true);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ShiningBlade:
                var charge = Charges.Find(c => c?.Source == caster);
                if (charge?.ChargeAOEs.Count > 0)
                    charge.ChargeAOEs.RemoveAt(0);
                break;
            case AID.BrightFlare:
                foreach (var c in Charges)
                    c.Spheres.RemoveAll(s => s.AlmostEqual(caster.Position, 2));
                ++NumCasts;
                break;
        }
    }

    // note: currently we initialize charges when we get sever icons, but we should be able to do that a bit earlier: PATE 1E43 happens ~1.1s before icons
    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID != (uint)IconID.SacredSever1)
            return;

        var a1 = BuildChargeInfo(module, OID.SerAdelphel);
        var a2 = BuildChargeInfo(module, OID.SerJanlenoux);
        if (Charges.Count == 2 && a1 == a2)
        {
            ChargeAngle = a1;
        }
        else
        {
            module.ReportError(this, "Failed to initialize charges");
        }
    }

    // returns angle between successive charges (>0 if CCW, <0 if CW, 0 on failure)
    private Angle BuildChargeInfo(BossModule module, OID oid)
    {
        var actor = module.Enemies(oid).FirstOrDefault();
        if (actor == null)
            return default;

        // so far I've only seen both enemies starting at (+-5, 0)
        if (!Utils.AlmostEqual(actor.Position.Z, module.Bounds.Center.Z, 1))
            return default;
        if (!Utils.AlmostEqual(MathF.Abs(actor.Position.X - module.Bounds.Center.X), 5, 1))
            return default;

        bool right = actor.Position.X > module.Bounds.Center.X;
        bool facingSouth = Utils.AlmostEqual(actor.Rotation.Rad, 0, 0.1f);
        bool cw = right == facingSouth;
        var res = new ChargeInfo(actor);
        var firstPointDir = actor.Rotation;
        var angleBetweenPoints = (cw ? -1 : 1) * 112.5f.Degrees();

        Func<Angle, WPos> posAt = dir => module.Bounds.Center + 21 * dir.ToDirection();
        var p0 = actor.Position;
        var p1 = posAt(firstPointDir);
        var p2 = posAt(firstPointDir + angleBetweenPoints);
        var p3 = posAt(firstPointDir + angleBetweenPoints * 2);

        Func<WPos, WPos, AOEInstance> chargeAOE = (from, to) => new(new AOEShapeRect((to - from).Length(), _chargeHalfWidth), from, Angle.FromDirection(to - from));
        res.ChargeAOEs.Add(chargeAOE(p0, p1));
        res.ChargeAOEs.Add(chargeAOE(p1, p2));
        res.ChargeAOEs.Add(chargeAOE(p2, p3));

        res.Spheres.Add(p0);
        res.Spheres.Add(WPos.Lerp(p0, p1, 0.5f));
        res.Spheres.Add(p1);
        res.Spheres.Add(WPos.Lerp(p1, p2, 1.0f / 3));
        res.Spheres.Add(WPos.Lerp(p1, p2, 2.0f / 3));
        res.Spheres.Add(p2);
        res.Spheres.Add(WPos.Lerp(p2, p3, 1.0f / 3));
        res.Spheres.Add(WPos.Lerp(p2, p3, 2.0f / 3));
        res.Spheres.Add(p3);

        Charges.Add(res);
        return angleBetweenPoints;
    }
}

// hints & assignments
class P2SanctityOfTheWard1Hints : BossComponent
{
    private P2SanctityOfTheWard1Sever? _sever;
    private P2SanctityOfTheWard1Flares? _flares;
    private bool _inited;
    private Angle _severStartDir;
    private bool _chargeEarly;
    private BitMask _groupEast; // 0 until initialized
    private string _groupSwapHints = "";

    public override void Init(BossModule module)
    {
        _sever = module.FindComponent<P2SanctityOfTheWard1Sever>();
        _flares = module.FindComponent<P2SanctityOfTheWard1Flares>();
    }

    public override void Update(BossModule module)
    {
        if (!_inited && _sever?.Source != null && _sever.Stacks.Count == 2 && _flares != null && _flares.ChargeAngle != default)
        {
            _inited = true;
            _severStartDir = Angle.FromDirection(_sever.Source.Position - module.Bounds.Center);

            var config = Service.Config.Get<DSW2Config>();
            _groupEast = config.P2SanctityGroups.BuildGroupMask(1, module.Raid);
            if (_groupEast.None())
            {
                _groupSwapHints = "unconfigured";
            }
            else
            {
                if (config.P2SanctityRelative && _severStartDir.Rad < 0)
                {
                    // swap groups for relative assignment if needed
                    _groupEast.Raw ^= 0xff;
                }

                var effRoles = Service.Config.Get<PartyRolesConfig>().EffectiveRolePerSlot(module.Raid);
                if (config.P2SanctitySwapRole == Role.None)
                {
                    AssignmentSwapWithRolePartner(module, effRoles, _sever.Stacks[0].Target, _severStartDir.Rad < 0);
                    AssignmentSwapWithRolePartner(module, effRoles, _sever.Stacks[1].Target, _severStartDir.Rad > 0);
                }
                else
                {
                    AssignmentReassignIfNeeded(module, _sever.Stacks[0].Target, _severStartDir.Rad < 0);
                    AssignmentReassignIfNeeded(module, _sever.Stacks[1].Target, _severStartDir.Rad > 0);
                    if (_groupEast.NumSetBits() != 4)
                    {
                        // to balance, unmarked player of designated role should swap
                        var (swapSlot, swapper) = module.Raid.WithSlot(true).FirstOrDefault(sa => sa.Item2 != _sever.Stacks[0].Target && sa.Item2 != _sever.Stacks[1].Target && effRoles[sa.Item1] == config.P2SanctitySwapRole);
                        if (swapper != null)
                        {
                            _groupEast.Toggle(swapSlot);
                            _groupSwapHints = swapper.Name;
                        }
                    }
                }

                if (_groupSwapHints.Length == 0)
                    _groupSwapHints = "none";
            }

            // second safe spot could be either 3rd or 5th explosion
            var severDirEast = _severStartDir;
            if (severDirEast.Rad < 0)
                severDirEast += 180.Degrees();
            bool severDiagonalSE = severDirEast.Rad < MathF.PI / 2;
            bool chargeCW = _flares.ChargeAngle.Rad < 0;
            _chargeEarly = severDiagonalSE == chargeCW;
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (movementHints != null && _groupEast.Any())
        {
            var from = actor.Position;
            var color = ArenaColor.Safe;
            foreach (var safespot in MovementHintOffsets(slot))
            {
                var to = module.Bounds.Center + safespot;
                movementHints.Add(from, to, color);
                from = to;
                color = ArenaColor.Danger;
            }
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var safespot in MovementHintOffsets(pcSlot).Take(1))
        {
            arena.AddCircle(module.Bounds.Center + safespot, 1, ArenaColor.Safe);
            if (_groupEast.None())
                arena.AddCircle(module.Bounds.Center - safespot, 1, ArenaColor.Safe); // if there are no valid assignments, draw spots for both groups
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (!_inited)
            return;
        if (_groupSwapHints.Length > 0)
            hints.Add($"Swap: {_groupSwapHints}");
        if (_flares != null && _flares.ChargeAngle != default)
            hints.Add($"Move: {(_flares.ChargeAngle.Rad < 0 ? "clockwise" : "counterclockwise")} {(_chargeEarly ? "early" : "late")}");
    }

    private void AssignmentReassignIfNeeded(BossModule module, Actor player, bool shouldGoEast)
    {
        int slot = module.Raid.FindSlot(player.InstanceID);
        if (shouldGoEast == _groupEast[slot])
            return; // target is already assigned to correct position, no need to swap
        _groupEast.Toggle(slot);
    }

    private void AssignmentSwapWithRolePartner(BossModule module, Role[] effRoles, Actor player, bool shouldGoEast)
    {
        int slot = module.Raid.FindSlot(player.InstanceID);
        if (shouldGoEast == _groupEast[slot])
            return; // target is already assigned to correct position, no need to swap
        var role = effRoles[slot];
        var (partnerSlot, partner) = module.Raid.WithSlot(true).Exclude(slot).FirstOrDefault(sa => effRoles[sa.Item1] == role);
        if (partner == null)
            return;

        _groupEast.Toggle(slot);
        _groupEast.Toggle(partnerSlot);

        if (_groupSwapHints.Length > 0)
            _groupSwapHints += ", ";
        _groupSwapHints += role.ToString();
    }

    private WDir SafeSpotOffset(int slot, Angle dirOffset)
    {
        var dir = _severStartDir + (_flares?.ChargeAngle.Rad < 0 ? -1 : 1) * dirOffset;
        if (dir.Rad < 0 == _groupEast[slot])
            dir += 180.Degrees();
        return 20 * dir.ToDirection();
    }

    private IEnumerable<WDir> MovementHintOffsets(int slot)
    {
        if (_inited && _flares?.Charges.Count == 2)
        {
            // second safe spot could be either 3rd or 5th explosion
            if (_flares.Charges[0].Spheres.Count > (_chargeEarly ? 6 : 4))
                yield return SafeSpotOffset(slot, _chargeEarly ? 15.Degrees() : 11.7f.Degrees());
            if (_flares.Charges[0].Spheres.Count > 0)
                yield return SafeSpotOffset(slot, 33.3f.Degrees());
        }
    }
}
