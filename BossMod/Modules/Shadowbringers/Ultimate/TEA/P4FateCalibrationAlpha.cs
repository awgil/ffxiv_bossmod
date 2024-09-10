namespace BossMod.Shadowbringers.Ultimate.TEA;

class P4FateCalibrationAlphaStillnessMotion(BossModule module) : Components.StayMove(module)
{
    public int NumCasts { get; private set; }
    private Requirement _first;
    private Requirement _second;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_first != Requirement.None)
            hints.Add($"Movement: {_first} -> {(_second != Requirement.None ? _second.ToString() : "???")}");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FateCalibrationAlphaOrdainedMotion1:
                _first = Requirement.Move;
                break;
            case AID.FateCalibrationAlphaOrdainedStillness1:
                _first = Requirement.Stay;
                break;
            case AID.FateCalibrationAlphaOrdainedMotion2:
                _second = Requirement.Move;
                break;
            case AID.FateCalibrationAlphaSacrament: // TODO: this is a hack, I've never seen stilness 2...
                if (_second == Requirement.None)
                    _second = Requirement.Stay;
                break;
            case AID.FateCalibrationAlphaResolveOrdainedMotion:
            case AID.FateCalibrationAlphaResolveOrdainedStillness:
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
        Array.Fill(PlayerStates, new(req, default));
    }
}

class P4FateCalibrationAlphaDebuffs(BossModule module) : Components.UniformStackSpread(module, 4, 30, 3, alwaysShowSpreads: true)
{
    public enum Debuff { None, Defamation, SharedSentence, AggravatedAssault }

    public Debuff[] Debuffs = new Debuff[PartyState.MaxPartySize];
    private readonly P4FateProjection? _proj = module.FindComponent<P4FateProjection>();
    private BitMask _avoidMask;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FateCalibrationAlphaDefamation:
                // applied to nearest projection
                var defamationSlot = _proj?.ProjectionOwner(Module.Enemies(OID.FateProjection).Closest(caster.Position)?.InstanceID ?? 0) ?? -1;
                if (defamationSlot >= 0)
                {
                    Debuffs[defamationSlot] = Debuff.Defamation;
                    var defamationTarget = Raid[defamationSlot];
                    if (defamationTarget != null)
                        AddSpread(defamationTarget, WorldState.FutureTime(20.1f));
                }
                break;
            case AID.FateCalibrationAlphaSharedSentence:
                var sharedSlot = _proj?.ProjectionOwner(spell.MainTargetID) ?? -1;
                if (sharedSlot >= 0)
                {
                    Debuffs[sharedSlot] = Debuff.SharedSentence;
                    var sharedTarget = Raid[sharedSlot];
                    if (sharedTarget != null)
                        AddStack(sharedTarget, WorldState.FutureTime(20.1f), _avoidMask); // note: avoid mask is typically empty here, since aggravated assaults happen later
                }
                break;
            case AID.FateCalibrationAlphaAggravatedAssault:
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

class P4FateCalibrationAlphaSacrament(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor caster, DateTime activation)> _casters = [];
    private WPos[]? _safespots;

    private static readonly AOEShapeCross _shape = new(100, 8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(_shape, c.caster.Position, c.caster.Rotation, c.activation));

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_safespots != null)
            movementHints.Add(actor.Position, _safespots[slot], ArenaColor.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_safespots != null)
            Arena.AddCircle(_safespots[pcSlot], 1, ArenaColor.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FateCalibrationAlphaSacrament:
                _casters.Add((caster, WorldState.FutureTime(18.1f)));
                if (_casters.Count == 3)
                    InitSafeSpots();
                break;
            case AID.FateCalibrationAlphaResolveSacrament:
                _casters.RemoveAll(c => c.caster == caster);
                ++NumCasts;
                break;
        }
    }

    private void InitSafeSpots()
    {
        var safeClone = Module.Enemies(OID.PerfectAlexander).FirstOrDefault(a => a != ((TEA)Module).PerfectAlex() && _casters.FindIndex(c => c.caster == a) < 0);
        var debuffs = Module.FindComponent<P4FateCalibrationAlphaDebuffs>();
        if (safeClone == null || debuffs == null)
            return;

        var dirToSafe = (safeClone.Position - Module.Center).Normalized();
        _safespots = new WPos[PartyState.MaxPartySize];
        for (int i = 0; i < _safespots.Length; ++i)
        {
            _safespots[i] = Module.Center + debuffs.Debuffs[i] switch
            {
                P4FateCalibrationAlphaDebuffs.Debuff.Defamation => 18 * dirToSafe,
                P4FateCalibrationAlphaDebuffs.Debuff.AggravatedAssault => -18 * dirToSafe + 3 * dirToSafe.OrthoR(),
                _ => -18 * dirToSafe + 3 * dirToSafe.OrthoL(),
            };
        }
    }
}
