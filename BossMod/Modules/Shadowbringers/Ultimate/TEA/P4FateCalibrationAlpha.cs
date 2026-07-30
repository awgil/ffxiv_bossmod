namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P4FateCalibrationAlphaStillnessMotion(BossModule module) : Components.StayMove(module)
{
    public int NumCasts;
    private Requirement _first;
    private Requirement _second;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_first != Requirement.None)
            hints.Add($"Movement: {_first} -> {(_second != Requirement.None ? _second.ToString() : "???")}");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FateCalibrationAlphaOrdainedMotion1:
                _first = Requirement.Move;
                break;
            case (uint)AID.FateCalibrationAlphaOrdainedStillness1:
                _first = Requirement.Stay;
                break;
            case (uint)AID.FateCalibrationAlphaOrdainedMotion2:
                _second = Requirement.Move;
                break;
            case (uint)AID.FateCalibrationAlphaSacrament: // TODO: this is a hack, I've never seen stilness 2...
                if (_second == Requirement.None)
                    _second = Requirement.Stay;
                break;
            case (uint)AID.FateCalibrationAlphaResolveOrdainedMotion:
            case (uint)AID.FateCalibrationAlphaResolveOrdainedStillness:
                ++NumCasts;
                ApplyNextRequirement();
                break;
        }
    }

    public void ApplyNextRequirement()
    {
        var req = NumCasts switch
        {
            0 => _first,
            1 => _second,
            _ => Requirement.None
        };
        Array.Fill(PlayerStates, new(req, WorldState.FutureTime(4.1d)));
    }
}

[SkipLocalsInit]
sealed class P4FateCalibrationAlphaDebuffs(BossModule module) : Components.UniformStackSpread(module, 4f, 30f, 3)
{
    public enum Debuff { None, Defamation, SharedSentence, AggravatedAssault }

    public Debuff[] Debuffs = new Debuff[PartyState.MaxPartySize];
    private readonly P4FateProjection? _proj = module.FindComponent<P4FateProjection>();
    private BitMask _avoidMask;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FateCalibrationAlphaDefamation:
                // applied to nearest projection
                var defamationSlot = _proj?.ProjectionOwner(Module.Enemies((uint)OID.FateProjection).Closest(caster.Position)?.InstanceID ?? 0) ?? -1;
                if (defamationSlot >= 0)
                {
                    Debuffs[defamationSlot] = Debuff.Defamation;
                    var defamationTarget = Raid[defamationSlot];
                    if (defamationTarget != null)
                        AddSpread(defamationTarget, WorldState.FutureTime(20.1d));
                }
                break;
            case (uint)AID.FateCalibrationAlphaSharedSentence:
                var sharedSlot = _proj?.ProjectionOwner(spell.MainTargetID) ?? -1;
                if (sharedSlot >= 0)
                {
                    Debuffs[sharedSlot] = Debuff.SharedSentence;
                    var sharedTarget = Raid[sharedSlot];
                    if (sharedTarget != null)
                        AddStack(sharedTarget, WorldState.FutureTime(20.1d), _avoidMask); // note: avoid mask is typically empty here, since aggravated assaults happen later
                }
                break;
            case (uint)AID.FateCalibrationAlphaAggravatedAssault:
                var avoidSlot = _proj?.ProjectionOwner(spell.MainTargetID) ?? -1;
                if (avoidSlot >= 0)
                {
                    Debuffs[avoidSlot] = Debuff.AggravatedAssault;
                    _avoidMask.Set(avoidSlot);
                    foreach (ref var s in Stacks.AsSpan())
                        s.ForbiddenPlayers = _avoidMask;
                }
                break;
        }
    }
}

[SkipLocalsInit]
sealed class P4FateCalibrationAlphaSacrament(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor caster, DateTime activation)> _casters = [];
    private WPos[]? _safespots;

    private static readonly AOEShapeCross _shape = new(100f, 8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var c = _casters[i];
            aoes[i] = new(_shape, c.caster.Position, c.caster.Rotation, c.activation);
        }
        return aoes;
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_safespots != null)
            movementHints.Add(actor.Position, _safespots[slot], Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_safespots != null)
            Arena.ZoneCircleOutline(_safespots[pcSlot], 1f, Colors.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FateCalibrationAlphaSacrament:
                _casters.Add((caster, WorldState.FutureTime(18.1d)));
                if (_casters.Count == 3)
                    InitSafeSpots();
                break;
            case (uint)AID.FateCalibrationAlphaResolveSacrament:
                _casters.RemoveAll(c => c.caster == caster);
                ++NumCasts;
                break;
        }
    }

    private void InitSafeSpots()
    {
        var safeClone = Module.Enemies((uint)OID.PerfectAlexander).FirstOrDefault(a => a != ((TEA)Module).PerfectAlex() && _casters.FindIndex(c => c.caster == a) < 0);
        var debuffs = Module.FindComponent<P4FateCalibrationAlphaDebuffs>();
        if (safeClone == null || debuffs == null)
            return;

        var dirToSafe = (safeClone.Position - Arena.Center).Normalized();
        _safespots = new WPos[PartyState.MaxPartySize];
        var len = _safespots.Length;
        for (var i = 0; i < _safespots.Length; ++i)
        {
            _safespots[i] = Arena.Center + debuffs.Debuffs[i] switch
            {
                P4FateCalibrationAlphaDebuffs.Debuff.Defamation => 18f * dirToSafe,
                P4FateCalibrationAlphaDebuffs.Debuff.AggravatedAssault => -18f * dirToSafe + 3f * dirToSafe.OrthoR(),
                _ => -18f * dirToSafe + 3f * dirToSafe.OrthoL(),
            };
        }
    }
}
