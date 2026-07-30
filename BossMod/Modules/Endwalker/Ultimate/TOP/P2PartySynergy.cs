namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P2PartySynergy(BossModule module) : CommonAssignments(module)
{
    public enum Glitch { Unknown, Mid, Remote }

    public Glitch ActiveGlitch;
    public bool EnableDistanceHints;
    private readonly TOPConfig _config = Service.Config.Get<TOPConfig>();

    protected override (GroupAssignmentUnique assignment, bool global) Assignments()
    {
        var config = Service.Config.Get<TOPConfig>();
        return (config.P2PartySynergyAssignments, config.P2PartySynergyGlobalPriority);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ActiveGlitch != Glitch.Unknown)
            hints.Add($"Glitch: {ActiveGlitch}");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (EnableDistanceHints && FindPartner(slot) is var partner && partner != null)
        {
            var distSq = (partner.Position - actor.Position).LengthSq();
            var range = DistanceRange;
            if (distSq < range.min * range.min)
                hints.Add("Move away from partner!");
            else if (distSq > range.max * range.max)
                hints.Add("Move closer to partner!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var partner = FindPartner(pcSlot);
        if (partner != null)
        {
            var distSq = (partner.Position - pc.Position).LengthSq();
            var range = DistanceRange;
            Arena.AddLine(pc.Position, partner.Position, distSq < range.min * range.min || distSq > range.max * range.max ? Colors.Danger : Colors.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.MidGlitch:
                ActiveGlitch = Glitch.Mid;
                break;
            case (uint)SID.RemoteGlitch:
                ActiveGlitch = Glitch.Remote;
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        // assuming standard 'blue-purple-orange-green' order
        var order = iconID switch
        {
            (uint)IconID.PartySynergyCross => 1,
            (uint)IconID.PartySynergySquare => 2,
            (uint)IconID.PartySynergyCircle => 3,
            (uint)IconID.PartySynergyTriangle => 4,
            _ => 0
        };
        Assign(actor, order);
    }

    private Actor? FindPartner(int slot)
    {
        var ps = PlayerStates[slot];
        var partnerSlot = ps.Order > 0 ? Array.FindIndex(PlayerStates, s => s.Order == ps.Order && s.Group != ps.Group) : -1;
        return Raid[partnerSlot];
    }

    private (float min, float max) DistanceRange => ActiveGlitch switch
    {
        Glitch.Mid => (20, 26),
        Glitch.Remote => (34, 50),
        _ => (0, 50)
    };

    // determine north => south order for player based on what glitch is active, used for flare stacks
    public int GetNorthSouthOrder(PlayerState st)
    {
        if (st.Group is < 1 or > 2)
            return 0;

        if (st.Group == 1 || ActiveGlitch == Glitch.Mid)
            return st.Order;

        return st.Order switch
        {
            1 => 4,
            2 => _config.P2PartySynergyG2ReverseAll ? 3 : 2,
            3 => _config.P2PartySynergyG2ReverseAll ? 2 : 3,
            4 => 1,
            _ => 0
        };
    }
}

sealed class P2PartySynergyDoubleAOEs(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> AOEs = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BeyondStrength or (uint)AID.EfficientBladework or (uint)AID.SuperliminalSteel or (uint)AID.OptimizedBlizzard)
            ++NumCasts;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id != 0x1E43)
            return;
        var pos = actor.Position.Quantized();
        var act = WorldState.FutureTime(5.1d);
        var rot = actor.Rotation;
        switch (actor.OID)
        {
            case (uint)OID.OmegaMHelper:
                if (actor.ModelState.ModelState == 4)
                {
                    AOEs.Add(new(P5OmegaDoubleAOEs.Shapes[0], pos, rot, act));
                }
                else
                {
                    AOEs.Add(new(P5OmegaDoubleAOEs.Shapes[1], pos, rot, act));
                }
                break;
            case (uint)OID.OmegaFHelper:
                if (actor.ModelState.ModelState == 4)
                {
                    AOEs.Add(new(P5OmegaDoubleAOEs.Shapes[2], pos, rot + 90f.Degrees(), act));
                    AOEs.Add(new(P5OmegaDoubleAOEs.Shapes[2], pos, rot - 90f.Degrees(), act));
                }
                else
                {
                    AOEs.Add(new(P5OmegaDoubleAOEs.Shapes[3], pos, rot, act));
                }
                break;
        }
    }
}

sealed class P2PartySynergyOptimizedFire : Components.UniformStackSpread
{
    public P2PartySynergyOptimizedFire(BossModule module) : base(module, default, 7f)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OptimizedFire)
            Spreads.Clear();
    }
}

sealed class P2PartySynergyOpticalLaser(BossModule module) : Components.GenericAOEs(module, (uint)AID.OpticalLaser)
{
    private readonly P2PartySynergy? _synergy = module.FindComponent<P2PartySynergy>();
    private readonly Actor? _source = module.Enemies((uint)OID.OpticalUnit).FirstOrDefault();
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(100f, 8f);

    public void Show()
    {
        _activation = WorldState.FutureTime(6.8d);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default && _source != null)
            return new AOEInstance[1] { new(_shape, _source.Position.Quantized(), _source.Rotation, _activation) };
        return [];
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_source, Colors.Object, true);
        var pos = AssignedPosition(pcSlot);
        if (pos != default)
            Arena.ZoneCircleOutline(Arena.Center + pos, 1f, Colors.Safe);
    }

    private WDir AssignedPosition(int slot)
    {
        if (_synergy == null || _source == null || _activation == default)
            return default;

        var ps = _synergy.PlayerStates[slot];
        if (ps.Order == 0 || ps.Group == 0 || _synergy.ActiveGlitch == P2PartySynergy.Glitch.Unknown)
            return default;

        var eyeOffset = _source.Position - Arena.Center;
        var toRelNorth = eyeOffset.Normalized();
        var order = _synergy.GetNorthSouthOrder(ps);
        var centerOffset = _synergy.ActiveGlitch == P2PartySynergy.Glitch.Remote && order is 2 or 3 ? 17.5f : 11;
        return 10f * (2.5f - order) * toRelNorth + centerOffset * (ps.Group == 1 ? toRelNorth.OrthoL() : toRelNorth.OrthoR());
    }
}

sealed class P2PartySynergyDischarger(BossModule module) : Components.GenericKnockback(module, (uint)AID.Discharger)
{
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        return new Knockback[1] { new(Arena.Center, 13f) }; // TODO: activation
    }
}

class P2PartySynergyEfficientBladework : Components.GenericAOEs
{
    private readonly P2PartySynergy? _synergy;
    private DateTime _activation;
    private readonly List<Actor> _sources = [];
    private int _firstStackSlot = -1;
    private BitMask _firstGroup;
    private string _swaps = "";
    private readonly TOPConfig _config = Service.Config.Get<TOPConfig>();

    private static readonly AOEShapeCircle _shape = new(10);

    public P2PartySynergyEfficientBladework(BossModule module) : base(module)
    {
        _synergy = module.FindComponent<P2PartySynergy>();
        _sources.AddRange(module.Enemies((uint)OID.OmegaF));
        // by default, use same group as for synergy
        if (_synergy != null)
            _firstGroup = Raid.WithSlot(true, true, true).WhereSlot(s => _synergy.PlayerStates[s].Group == 1).Mask();
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            var count = _sources.Count;
            var aoes = new AOEInstance[count];
            for (var i = 0; i < count; ++i)
                aoes[i] = new(_shape, _sources[i].Position.Quantized(), default, _activation);
            return aoes;
        }
        return [];
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_swaps.Length > 0)
            hints.Add($"Swaps: {_swaps}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var pos = AssignedPosition(pcSlot);
        if (pos != default)
            Arena.ZoneCircleOutline(Arena.Center + pos, 1f, Colors.Safe);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && actor.OID == (uint)OID.OmegaMHelper)
            _sources.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.EfficientBladework:
                ++NumCasts;
                break;
            case (uint)AID.OpticalLaser:
                _activation = WorldState.FutureTime(9.8d);
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Spotlight && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && _synergy != null)
        {
            if (_firstStackSlot < 0)
            {
                _firstStackSlot = slot;
            }
            else
            {
                // as soon as we have two stacks, check whether they are from same group - if so, we need adjusts
                var s1 = _synergy.PlayerStates[_firstStackSlot];
                var s2 = _synergy.PlayerStates[slot];
                if (s1.Group == s2.Group)
                {
                    // need adjust
                    var s1Order = _synergy.GetNorthSouthOrder(s1);
                    var s2Order = _synergy.GetNorthSouthOrder(s2);
                    var adjustOrder = _config.P2PartySynergyStackSwapSouth
                        ? s1Order > s2Order ? s1.Order : s2.Order // south = higher order will swap
                        : s1Order > s2Order ? s2.Order : s1.Order; // north = lower

                    for (var s = 0; s < _synergy.PlayerStates.Length; ++s)
                    {
                        if (_synergy.PlayerStates[s].Order == adjustOrder)
                        {
                            _firstGroup.Toggle(s);
                            if (_swaps.Length > 0)
                                _swaps += ", ";
                            _swaps += Raid[s]?.Name ?? "";
                        }
                    }
                }
                else
                {
                    _swaps = "None";
                }
            }
        }
    }

    private WDir AssignedPosition(int slot)
    {
        if (_activation == default || _synergy == null || _sources.Count == 0)
            return default;

        // assumption: first source (F) is our relative north, G1 always goes to relative west, G2 goes to relative S/E depending on glitch
        var relNorth = 1.4f * (_sources[0].Position - Arena.Center);
        return _firstGroup[slot] ? relNorth.OrthoL() : _synergy.ActiveGlitch == P2PartySynergy.Glitch.Mid ? -relNorth : relNorth.OrthoR();
    }
}

sealed class P2PartySynergySpotlight(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4, 4)
{
    private readonly List<Actor> _stackTargets = []; // don't show anything until knockbacks are done, to reduce visual clutter

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Spotlight)
            _stackTargets.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Discharger:
                AddStacks(_stackTargets);
                break;
            case (uint)AID.Spotlight:
                Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;
        }
    }
}
