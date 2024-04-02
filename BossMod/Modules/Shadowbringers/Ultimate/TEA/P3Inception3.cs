namespace BossMod.Shadowbringers.Ultimate.TEA;

class P3Inception3Sacrament : Components.GenericAOEs
{
    public bool Active => _source != null;

    private Actor? _source;
    private DateTime _activation;
    private static readonly AOEShapeCross _shape = new(100, 8);

    public P3Inception3Sacrament() : base(ActionID.MakeSpell(AID.SacramentInception)) { }

    public override void Init(BossModule module)
    {
        // note: boss moves to position around the component activation time
        _source = ((TEA)module).AlexPrime();
        _activation = module.WorldState.CurrentTime.AddSeconds(4.1f);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_shape, _source.Position, _source.Rotation, _activation);
    }
}

class P3Inception3Debuffs : Components.GenericStackSpread
{
    private Actor? _sharedSentence;
    private BitMask _avoid;
    private BitMask _tethered;
    private bool _inited; // we init stuff on first update, since component is activated when statuses are already applied

    public override void Update(BossModule module)
    {
        if (!_inited)
        {
            _inited = true;
            if (_sharedSentence != null)
                Stacks.Add(new(_sharedSentence, 4, 3, int.MaxValue, module.WorldState.CurrentTime.AddSeconds(4), _avoid));
        }
        base.Update(module);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);
        if (_tethered[slot] && FindPartner(module, slot) is var partner && partner != null && (partner.Position - actor.Position).LengthSq() < 30 * 30)
            hints.Add("Stay farther from partner!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);

        if (_tethered[pcSlot] && FindPartner(module, pcSlot) is var partner && partner != null)
            arena.AddLine(pc.Position, partner.Position, (partner.Position - pc.Position).LengthSq() < 30 * 30 ? ArenaColor.Danger : ArenaColor.Safe);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.AggravatedAssault:
                _avoid.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.SharedSentence:
                _sharedSentence = actor;
                break;
            case SID.RestrainingOrder:
                _tethered.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    private Actor? FindPartner(BossModule module, int slot)
    {
        var remaining = _tethered;
        remaining.Clear(slot);
        return remaining.Any() ? module.Raid[remaining.LowestSetBit()] : null;
    }
}
