namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P2SanctityOfTheWard1Gaze : DragonsGaze
{
    public P2SanctityOfTheWard1Gaze(BossModule module) : base(module, (uint)OID.BossP2, 9.4d)
    {
        EnableHints = true;
    }
}

// sacred sever - distance-based shared damage on 1/2/1/2 markers
sealed class P2SanctityOfTheWard1Sever(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4)
{
    public int NumCasts;
    public Actor? Source = module.Enemies((uint)OID.SerZephirin).FirstOrDefault();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (Stacks.Count == 2 && Source != null)
        {
            Arena.Actor(Source, Colors.Enemy, true);
            Arena.AddLine(Source.Position, Stacks.Ref(NumCasts & 1).Target.Position);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SacredSever && ++NumCasts >= 4)
            Stacks.Clear();
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.SacredSever1:
                Stacks.Insert(0, new(actor, StackRadius, MinStackSize, MaxStackSize));
                break;
            case (uint)IconID.SacredSever2:
                AddStack(actor);
                break;
        }
    }
}

// shining blade (charges that leave orbs) + flares (their explosions)
sealed class P2SanctityOfTheWard1Flares(BossModule module) : Components.GenericAOEs(module, (uint)AID.BrightFlare, "GTFO from charges and spheres!")
{
    public class ChargeInfo(Actor source)
    {
        public Actor Source = source;
        public List<AOEInstance> ChargeAOEs = [];
        public List<WPos> Spheres = [];
    }

    public List<ChargeInfo> Charges = [];
    public Angle ChargeAngle; // 0 if charges are not active or on failure, <0 if CW, >0 if CCW

    private const float _chargeHalfWidth = 3f;
    private static readonly AOEShapeCircle _brightflareShape = new(9);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = 0;
        var countC = Charges.Count;
        for (var i = 0; i < countC; ++i)
        {
            var c = Charges[i];
            count += c.ChargeAOEs.Count + Math.Min(c.Spheres.Count, 6);
        }

        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        var index = 0;

        for (var i = 0; i < countC; ++i)
        {
            var c = Charges[i];
            var charges = c.ChargeAOEs;
            var spheres = c.Spheres;
            var countCC = charges.Count;
            for (var j = 0; j < countCC; ++j)
                aoes[index++] = charges[j];
            var countS = spheres.Count;
            var max = countS > 6 ? 6 : countS;
            for (var k = 0; k < max; ++k)
                aoes[index++] = new AOEInstance(_brightflareShape, spheres[k]);
        }
        return aoes;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        List<Actor> enemies = [];
        var count = Charges.Count;
        for (var i = 0; i < count; ++i)
        {
            var charge = Charges[i];
            if (charge.ChargeAOEs.Count != 0)
            {
                enemies.Add(charge.Source);
            }
        }

        Arena.Actors(enemies, allowDeadAndUntargetable: true);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ShiningBlade:
                var charge = Charges.Find(c => c?.Source == caster);
                if (charge?.ChargeAOEs.Count > 0)
                    charge.ChargeAOEs.RemoveAt(0);
                break;
            case (uint)AID.BrightFlare:
                foreach (var c in Charges)
                    c.Spheres.RemoveAll(s => s.AlmostEqual(caster.Position, 2f));
                ++NumCasts;
                break;
        }
    }

    // note: currently we initialize charges when we get sever icons, but we should be able to do that a bit earlier: PATE 1E43 happens ~1.1s before icons
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID != (uint)IconID.SacredSever1)
            return;

        var a1 = BuildChargeInfo((uint)OID.SerAdelphel);
        var a2 = BuildChargeInfo((uint)OID.SerJanlenoux);
        if (Charges.Count == 2 && a1 == a2)
        {
            ChargeAngle = a1;
        }
        else
        {
            ReportError("Failed to initialize charges");
        }
    }

    // returns angle between successive charges (>0 if CCW, <0 if CW, 0 on failure)
    private Angle BuildChargeInfo(uint oid)
    {
        var actors = Module.Enemies(oid);
        var actor = actors.Count != 0 ? actors[0] : null;
        if (actor == null)
            return default;

        // so far I've only seen both enemies starting at (+-5, 0)
        if (!Utils.AlmostEqual(actor.PosRot.Z, Arena.Center.Z, 1f))
            return default;
        if (!Utils.AlmostEqual(Math.Abs(actor.PosRot.X - Arena.Center.X), 5f, 1f))
            return default;

        var right = actor.PosRot.X > Arena.Center.X;
        var facingSouth = Utils.AlmostEqual(actor.Rotation.Rad, default, 0.1f);
        var cw = right == facingSouth;
        var res = new ChargeInfo(actor);
        var firstPointDir = actor.Rotation;
        var angleBetweenPoints = (cw ? -1f : 1f) * 112.5f.Degrees();

        WPos posAt(Angle dir) => Arena.Center + 21f * dir.ToDirection();
        var p0 = actor.Position;
        var p1 = posAt(firstPointDir);
        var p2 = posAt(firstPointDir + angleBetweenPoints);
        var p3 = posAt(firstPointDir + angleBetweenPoints * 2);

        AOEInstance chargeAOE(WPos from, WPos to) => new(new AOEShapeRect((to - from).Length(), _chargeHalfWidth), from, Angle.FromDirection(to - from));
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
sealed class P2SanctityOfTheWard1Hints(BossModule module) : BossComponent(module)
{
    private readonly P2SanctityOfTheWard1Sever? _sever = module.FindComponent<P2SanctityOfTheWard1Sever>();
    private readonly P2SanctityOfTheWard1Flares? _flares = module.FindComponent<P2SanctityOfTheWard1Flares>();
    private readonly DSW2Config config = Service.Config.Get<DSW2Config>();
    private bool _inited;
    private Angle _severStartDir;
    private bool _chargeEarly;
    private BitMask _groupEast; // 0 until initialized
    private string _groupSwapHints = "";

    public override void Update()
    {
        if (!_inited && _sever?.Source != null && _sever.Stacks.Count == 2 && _flares != null && _flares.ChargeAngle != default)
        {
            _inited = true;
            _severStartDir = Angle.FromDirection(_sever.Source.Position - Arena.Center);

            _groupEast = config.P2SanctityGroups.BuildGroupMask(1, Raid);
            if (_groupEast.None())
            {
                _groupSwapHints = "unconfigured";
            }
            else
            {
                var startDir = _severStartDir.Rad;
                if (config.P2SanctityRelative && startDir < 0f)
                {
                    // swap groups for relative assignment if needed
                    _groupEast.Raw ^= 0xff;
                }
                ref var s0 = ref _sever.Stacks.Ref(0);
                ref var s1 = ref _sever.Stacks.Ref(1);
                var effRoles = Service.Config.Get<PartyRolesConfig>().EffectiveRolePerSlot(Raid);
                if (config.P2SanctitySwapRole == Role.None)
                {
                    AssignmentSwapWithRolePartner(effRoles, s0.Target, startDir < 0f);
                    AssignmentSwapWithRolePartner(effRoles, s1.Target, startDir > 0f);
                }
                else
                {
                    AssignmentReassignIfNeeded(s0.Target, startDir < 0f);
                    AssignmentReassignIfNeeded(s1.Target, startDir > 0f);
                    if (_groupEast.NumSetBits() != 4)
                    {
                        // to balance, unmarked player of designated role should swap
                        var (swapSlot, swapper) = Raid.WithSlot(true, true, true).FirstOrDefault(sa => sa.Item2 != _sever.Stacks[0].Target && sa.Item2 != _sever.Stacks[1].Target && effRoles[sa.Item1] == config.P2SanctitySwapRole);
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
            if (severDirEast.Rad < 0f)
                severDirEast += 180f.Degrees();
            var severDiagonalSE = severDirEast.Rad < Angle.HalfPi;
            var chargeCW = _flares.ChargeAngle.Rad < 0f;
            _chargeEarly = severDiagonalSE == chargeCW;
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_groupEast.Any())
        {
            var from = actor.Position;
            var color = Colors.Safe;
            foreach (var safespot in MovementHintOffsets(slot))
            {
                var to = Arena.Center + safespot;
                movementHints.Add(from, to, color);
                from = to;
                color = Colors.Danger;
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!_inited)
            return;
        if (_groupSwapHints.Length > 0)
            hints.Add($"Swap: {_groupSwapHints}");
        if (_flares != null && _flares.ChargeAngle != default)
            hints.Add($"Move: {(_flares.ChargeAngle.Rad < 0f ? "clockwise" : "counterclockwise")} {(_chargeEarly ? "early" : "late")}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var safespot in MovementHintOffsets(pcSlot).Take(1))
        {
            Arena.ZoneCircleOutline(Arena.Center + safespot, 1f, Colors.Safe);
            if (_groupEast.None())
                Arena.ZoneCircleOutline(Arena.Center - safespot, 1f, Colors.Safe); // if there are no valid assignments, draw spots for both groups
        }
    }

    private void AssignmentReassignIfNeeded(Actor player, bool shouldGoEast)
    {
        var slot = Raid.FindSlot(player.InstanceID);
        if (shouldGoEast == _groupEast[slot])
            return; // target is already assigned to correct position, no need to swap
        _groupEast.Toggle(slot);
    }

    private void AssignmentSwapWithRolePartner(Role[] effRoles, Actor player, bool shouldGoEast)
    {
        var slot = Raid.FindSlot(player.InstanceID);
        if (shouldGoEast == _groupEast[slot])
            return; // target is already assigned to correct position, no need to swap
        var role = effRoles[slot];
        var (partnerSlot, partner) = Raid.WithSlot(true, true, true).Exclude(slot).FirstOrDefault(sa => effRoles[sa.Item1] == role);
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
        var dir = _severStartDir + (_flares?.ChargeAngle.Rad < 0f ? -1f : 1f) * dirOffset;
        if ((dir.Rad < 0) == _groupEast[slot])
            dir += 180f.Degrees();
        return 20f * dir.ToDirection();
    }

    private List<WDir> MovementHintOffsets(int slot)
    {
        if (_inited && _flares?.Charges.Count == 2)
        {
            var hints = new List<WDir>(1);
            var count = _flares.Charges[0].Spheres.Count;
            // second safe spot could be either 3rd or 5th explosion
            if (count > (_chargeEarly ? 6 : 4))
                hints.Add(SafeSpotOffset(slot, _chargeEarly ? 15f.Degrees() : 11.7f.Degrees()));
            if (count > 0)
                hints.Add(SafeSpotOffset(slot, 33.3f.Degrees()));
            return hints;
        }
        return [];
    }
}
