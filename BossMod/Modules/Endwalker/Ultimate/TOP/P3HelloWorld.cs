namespace BossMod.Endwalker.Ultimate.TOP;

class P3HelloWorld : Components.GenericTowers
{
    public enum PlayerRole { None = -1, Defamation, RemoteTether, Stack, LocalTether }
    public enum TowerColor { None, Red, Blue }

    public int NumRotExplodes { get; private set; }
    public int NumTetherBreaks { get; private set; }
    private PlayerRole[] _initialRoles = Utils.MakeArray(PartyState.MaxPartySize, PlayerRole.None);
    private TowerColor[] _initialRots = new TowerColor[PartyState.MaxPartySize];
    private TowerColor _defamationTowerColor;
    private BitMask _defamationTowers;
    private BitMask _defamationRotDone;
    private BitMask _stackRotDone;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_initialRoles[slot] != PlayerRole.None && NumCasts < 16)
            hints.Add($"Next role: {RoleForNextTowers(slot)}", false);

        if (Towers.Count == 4)
        {
            switch (RoleForNextTowers(slot))
            {
                case PlayerRole.Defamation:
                    if (module.Raid.WithSlot().InRadiusExcluding(actor, 20).WhereSlot(s => RoleForNextTowers(s) != PlayerRole.LocalTether).Any())
                        hints.Add("GTFO from others!");
                    break;
                case PlayerRole.RemoteTether:
                    if (module.Raid.WithSlot().InRadiusExcluding(actor, 20).WhereSlot(s => RoleForNextTowers(s) == PlayerRole.Defamation).Any())
                        hints.Add("GTFO from defamation!");
                    if (module.Raid.WithSlot().InRadiusExcluding(actor, 5).WhereSlot(s => RoleForNextTowers(s) == PlayerRole.Stack).Count() != 1)
                        hints.Add("Stay near one stack!");
                    break;
                case PlayerRole.Stack:
                    if (module.Raid.WithSlot().InRadiusExcluding(actor, 20).WhereSlot(s => RoleForNextTowers(s) == PlayerRole.Defamation).Any())
                        hints.Add("GTFO from defamation!");
                    if (module.Raid.WithSlot().InRadiusExcluding(actor, 5).WhereSlot(s => RoleForNextTowers(s) == PlayerRole.RemoteTether).Count() != 1)
                        hints.Add("Stay near one tether!");
                    break;
                case PlayerRole.LocalTether:
                    if (NumCasts < 12)
                    {
                        if (module.Raid.WithSlot().InRadiusExcluding(actor, 20).WhereSlot(s => RoleForNextTowers(s) == PlayerRole.Defamation).Count() != 1)
                            hints.Add("Stay inside one defamation!");
                    }
                    else
                    {
                        if (module.Raid.WithSlot().InRadiusExcluding(actor, 20).WhereSlot(s => RoleForNextTowers(s) == PlayerRole.Defamation).Any())
                            hints.Add("GTFO from defamation!");
                        // TODO: they don't have to share the stack, right?
                    }
                    break;
            }
        }
        else if (NumRotExplodes < NumCasts)
        {
            if (PendingRot(slot))
            {
                if (module.Raid.WithoutSlot().InRadiusExcluding(actor, 5).Any())
                    hints.Add("GTFO from raid!");
            }
            else
            {
                // TODO: hint to grab rot?..
                if (module.Raid.WithSlot(true).WhereSlot(PendingRot).InRadius(actor.Position, 5).Any())
                    hints.Add("GTFO from rots!");
            }
        }

        base.AddHints(module, slot, actor, hints, movementHints);
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_defamationTowerColor != TowerColor.None && NumCasts < 16)
            hints.Add($"Defamation color: {_defamationTowerColor}");
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var initialPCRole = _initialRoles[pcSlot];
        var initialPlayerRole = _initialRoles[playerSlot];
        if (initialPCRole == PlayerRole.None)
            return PlayerPriority.Irrelevant; // mechanic not started yet
        if (initialPCRole == initialPlayerRole)
        {
            customColor = ArenaColor.Vulnerable;
            return PlayerPriority.Interesting; // partner
        }
        var avoidPlayer = (RoleForNextTowers(initialPCRole), RoleForNextTowers(initialPlayerRole)) switch
        {
            (PlayerRole.Defamation, _) => !_defamationRotDone[playerSlot],
            (PlayerRole.Stack, _) => !_stackRotDone[playerSlot],
            (_, PlayerRole.Defamation) => !_defamationRotDone[pcSlot],
            (_, PlayerRole.Stack) => !_stackRotDone[pcSlot],
            _ => false,
        };
        return avoidPlayer ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);

        if (Towers.Count == 4)
        {
            // draw defamations & stacks
            var pcRole = RoleForNextTowers(pcSlot);
            foreach (var (i, p) in module.Raid.WithSlot(true))
            {
                var (radius, share) = RoleForNextTowers(i) switch
                {
                    PlayerRole.Defamation => (20, NumCasts < 12 ? PlayerRole.LocalTether : PlayerRole.None),
                    PlayerRole.Stack => (5, PlayerRole.RemoteTether),
                    _ => (0, PlayerRole.None)
                };
                if (radius != 0)
                    arena.AddCircle(p.Position, radius, pcRole == share ? ArenaColor.Safe : ArenaColor.Danger);
            }

            // draw safespots for next towers
            foreach (var p in PositionsForTowers(module, pcSlot))
                arena.AddCircle(p, 1, ArenaColor.Safe);
        }
        else if (NumRotExplodes < NumCasts)
        {
            // draw rot 'spreads' (rots will explode on players who used to have defamation/stack role and thus now have one of the tether roles)
            foreach (var (i, p) in module.Raid.WithSlot(true))
                if (PendingRot(i))
                    arena.AddCircle(p.Position, 5, ArenaColor.Danger);
        }

        if (NumTetherBreaks < 16)
        {
            // draw tether to partner, if any (TODO: color by risk, need to determine break distances...)
            //bool useRoleForNextTowers = NumTetherBreaks >= NumCasts;
            //var pcRole = RoleForNextTowers(pcSlot, useRoleForNextTowers ? 0 : -1);
            //if (pcRole is PlayerRole.RemoteTether or PlayerRole.LocalTether &&  is var partner && partner != null)
            //    arena.AddLine(pc.Position, partner.Position, ArenaColor.Danger);
            var partner = module.Raid[PartnerSlot(pcSlot)];
            if (partner != null && (pc.Tether.Target == partner.InstanceID || partner.Tether.Target == pc.InstanceID))
                arena.AddLine(pc.Position, partner.Position, ArenaColor.Danger);
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.HWPrepStack:
                AssignRole(module, actor, PlayerRole.Stack);
                break;
            case SID.HWPrepDefamation:
                AssignRole(module, actor, PlayerRole.Defamation);
                break;
            case SID.HWPrepRedRot:
                AssignRot(module, actor, TowerColor.Red);
                break;
            case SID.HWPrepBlueRot:
                AssignRot(module, actor, TowerColor.Blue);
                break;
            case SID.HWPrepLocalTether:
                if ((status.ExpireAt - module.WorldState.CurrentTime).TotalSeconds < 30)
                    AssignRole(module, actor, PlayerRole.LocalTether);
                break;
            case SID.HWPrepRemoteTether:
                if ((status.ExpireAt - module.WorldState.CurrentTime).TotalSeconds < 30)
                    AssignRole(module, actor, PlayerRole.RemoteTether);
                break;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var color = (AID)spell.Action.ID switch
        {
            AID.HWRedTower => TowerColor.Red,
            AID.HWBlueTower => TowerColor.Blue,
            _ => TowerColor.None
        };
        if (color != TowerColor.None)
        {
            var isDefamationTower = color == _defamationTowerColor;
            var soakerRole = isDefamationTower ? PlayerRole.Defamation : PlayerRole.Stack;
            _defamationTowers[Towers.Count] = isDefamationTower; // note: this works, because tower casts never overlap
            Towers.Add(new(caster.Position, 6, forbiddenSoakers: module.Raid.WithSlot(true).WhereSlot(s => RoleForNextTowers(s) != soakerRole).Mask()));
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HWRedTower or AID.HWBlueTower)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
            ++NumCasts;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HWRedRot:
                if (_defamationTowerColor == TowerColor.Red)
                    _defamationRotDone.Set(module.Raid.FindSlot(spell.MainTargetID));
                else
                    _stackRotDone.Set(module.Raid.FindSlot(spell.MainTargetID));
                ++NumRotExplodes;
                break;
            case AID.HWBlueRot:
                if (_defamationTowerColor == TowerColor.Blue)
                    _defamationRotDone.Set(module.Raid.FindSlot(spell.MainTargetID));
                else
                    _stackRotDone.Set(module.Raid.FindSlot(spell.MainTargetID));
                ++NumRotExplodes;
                break;
            case AID.HWTetherBreak:
            case AID.HWTetherFail:
                ++NumTetherBreaks;
                break;
        }
    }

    private void AssignRole(BossModule module, Actor actor, PlayerRole role)
    {
        var slot = module.Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
        {
            module.ReportError(this, $"Failed to find slot for {actor.InstanceID:X}");
            return;
        }

        if (_initialRoles[slot] == PlayerRole.None)
        {
            _initialRoles[slot] = role;
            InitDefamationTowers(module, slot);
        }
        else if (_initialRoles[slot] != role)
        {
            module.ReportError(this, $"Unexpected role reassign: #{slot} {_initialRoles[slot]} -> {role}");
        }
    }

    private void AssignRot(BossModule module, Actor actor, TowerColor color)
    {
        var slot = module.Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
        {
            module.ReportError(this, $"Failed to find slot for {actor.InstanceID:X}");
            return;
        }

        if (_initialRots[slot] == TowerColor.None)
        {
            _initialRots[slot] = color;
            InitDefamationTowers(module, slot);
        }
        else if (_initialRots[slot] != color)
        {
            module.ReportError(this, $"Unexpected rot reassign: #{slot} {_initialRots[slot]} -> {color}");
        }
    }

    private void InitDefamationTowers(BossModule module, int slot)
    {
        var role = _initialRoles[slot];
        var color = _initialRots[slot];
        if (role == PlayerRole.None || color == TowerColor.None)
            return; // not interesting (yet?)

        if (role is PlayerRole.LocalTether or PlayerRole.RemoteTether)
        {
            module.ReportError(this, $"Unexpected rot on tethered player {slot}");
            return;
        }

        // calculate defamation color
        if (role == PlayerRole.Stack)
            color = color == TowerColor.Red ? TowerColor.Blue : TowerColor.Red;

        if (_defamationTowerColor == TowerColor.None)
            _defamationTowerColor = color;
        else if (_defamationTowerColor != color)
            module.ReportError(this, $"Unexpected defamation color change: {_defamationTowerColor} -> {color}");
    }

    private int NextTowerOrder => NumCasts >> 2;
    private PlayerRole RoleForNextTowers(PlayerRole r, int offset = 0) => (PlayerRole)(((int)r + NextTowerOrder + offset) & 3);
    private PlayerRole RoleForNextTowers(int slot, int offset = 0) => RoleForNextTowers(_initialRoles[slot], offset);

    private bool PendingRot(int slot) => RoleForNextTowers(slot) switch
    {
        PlayerRole.RemoteTether => !_defamationRotDone[slot],
        PlayerRole.LocalTether => !_stackRotDone[slot],
        _ => false
    };

    private int PartnerSlot(int slot)
    {
        var role = _initialRoles[slot];
        if (role == PlayerRole.None)
            return -1;
        for (int i = 0; i < _initialRoles.Length; ++i)
            if (i != slot && _initialRoles[i] == role)
                return i;
        return -1;
    }

    private IEnumerable<WPos> PositionsForTowers(BossModule module, int slot)
    {
        // find midpoint for defamation towers
        WDir defamationMid = default;
        foreach (int i in _defamationTowers.SetBits())
            defamationMid += Towers[i].Position - module.Bounds.Center;
        var defamationMidDir = Angle.FromDirection(defamationMid);

        switch (RoleForNextTowers(slot))
        {
            case PlayerRole.Defamation:
                // max melee at defamation towers
                yield return module.Bounds.Center + 15.5f * (defamationMidDir - 45.Degrees()).ToDirection();
                yield return module.Bounds.Center + 15.5f * (defamationMidDir + 45.Degrees()).ToDirection();
                break;
            case PlayerRole.RemoteTether:
                // hitbox radius, between towers (r=7) => angle delta 2*asin(3.5/12.5f) = 33 degrees
                yield return module.Bounds.Center - 12.5f * (defamationMidDir - 12.Degrees()).ToDirection();
                yield return module.Bounds.Center - 12.5f * (defamationMidDir + 12.Degrees()).ToDirection();
                break;
            case PlayerRole.Stack:
                // hitbox radius, at the inner edge of the tower (r=5) => angle delta 2*asin(2.5/12.5f) = 23 degrees
                yield return module.Bounds.Center - 12.5f * (defamationMidDir - 25.Degrees()).ToDirection();
                yield return module.Bounds.Center - 12.5f * (defamationMidDir + 25.Degrees()).ToDirection();
                break;
            case PlayerRole.LocalTether:
                if (NumCasts < 12)
                {
                    // max melee outside defamation towers (assuming 15.5f for both defamation and target and distance 7 between, angle between them is 2*asin(3.5/15.5) = 26 degrees
                    yield return module.Bounds.Center + 15.5f * (defamationMidDir - 75.Degrees()).ToDirection();
                    yield return module.Bounds.Center + 15.5f * (defamationMidDir + 75.Degrees()).ToDirection();
                }
                else
                {
                    // same as tethers sharing stack
                    yield return module.Bounds.Center - 12.5f * (defamationMidDir - 12.Degrees()).ToDirection();
                    yield return module.Bounds.Center - 12.5f * (defamationMidDir + 12.Degrees()).ToDirection();
                }
                break;
        }
    }
}
