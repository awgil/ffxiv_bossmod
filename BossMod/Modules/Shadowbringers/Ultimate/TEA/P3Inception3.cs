namespace BossMod.Shadowbringers.Ultimate.TEA;

class P3Inception3Sacrament : Components.GenericAOEs
{
    public bool Active => _source != null;

    private Actor? _source;
    private DateTime _activation;
    private static readonly AOEShapeCross _shape = new(100, 8);

    public P3Inception3Sacrament(BossModule module) : base(module, ActionID.MakeSpell(AID.SacramentInception))
    {
        // note: boss moves to position around the component activation time
        _source = ((TEA)module).AlexPrime();
        _activation = WorldState.FutureTime(4.1f);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_shape, _source.Position, _source.Rotation, _activation);
    }
}

class P3Inception3Debuffs(BossModule module) : Components.GenericStackSpread(module)
{
    private Actor? _sharedSentence;
    private BitMask _avoid;
    private BitMask _tethered;
    private bool _inited; // we init stuff on first update, since component is activated when statuses are already applied

    public override void Update()
    {
        if (!_inited)
        {
            _inited = true;
            if (_sharedSentence != null)
                Stacks.Add(new(_sharedSentence, 4, 3, int.MaxValue, WorldState.FutureTime(4), _avoid));
        }
        base.Update();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_tethered[slot] && FindPartner(slot) is var partner && partner != null && (partner.Position - actor.Position).LengthSq() < 30 * 30)
            hints.Add("Stay farther from partner!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (_tethered[pcSlot] && FindPartner(pcSlot) is var partner && partner != null)
            Arena.AddLine(pc.Position, partner.Position, (partner.Position - pc.Position).LengthSq() < 30 * 30 ? ArenaColor.Danger : ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.AggravatedAssault:
                _avoid.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.SharedSentence:
                _sharedSentence = actor;
                break;
            case SID.RestrainingOrder:
                _tethered.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    private Actor? FindPartner(int slot)
    {
        var remaining = _tethered;
        remaining.Clear(slot);
        return remaining.Any() ? Raid[remaining.LowestSetBit()] : null;
    }
}
