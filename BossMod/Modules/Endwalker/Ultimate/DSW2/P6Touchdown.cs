namespace BossMod.Endwalker.Ultimate.DSW2;

class P6Touchdown : Components.GenericAOEs
{
    private static readonly AOEShapeCircle _shape = new(20); // TODO: verify falloff

    public P6Touchdown() : base(ActionID.MakeSpell(AID.TouchdownAOE)) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        // TODO: activation
        yield return new(_shape, module.Bounds.Center);
        yield return new(_shape, module.Bounds.Center + new WDir(0, 25));
    }
}

class P6TouchdownCauterize : BossComponent
{
    private Actor? _nidhogg;
    private Actor? _hraesvelgr;
    private BitMask _boiling;
    private BitMask _freezing;

    private static readonly AOEShapeRect _shape = new(80, 11);

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        bool nidhoggSide = actor.Position.X < module.Bounds.Center.X; // note: assume nidhogg cleaves whole left side, hraes whole right side
        var forbiddenMask = nidhoggSide ? _boiling : _freezing;
        if (forbiddenMask[slot])
            hints.Add("GTFO from wrong side!");

        // note: assume both dragons are always at north side
        bool isClosest = module.Raid.WithoutSlot().Where(p => (p.Position.X < module.Bounds.Center.X) == nidhoggSide).MinBy(p => p.PosRot.Z) == actor;
        bool shouldBeClosest = actor.Role == Role.Tank;
        if (isClosest != shouldBeClosest)
            hints.Add(shouldBeClosest ? "Move closer to dragons!" : "Move away from dragons!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_boiling[pcSlot] && _nidhogg != null)
            _shape.Draw(arena, _nidhogg);
        if (_freezing[pcSlot] && _hraesvelgr != null)
            _shape.Draw(arena, _hraesvelgr);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Boiling:
                _boiling.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.Freezing:
                _freezing.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CauterizeN:
                _nidhogg = caster;
                break;
            case AID.CauterizeH:
                _hraesvelgr = caster;
                break;
        }
    }
}
