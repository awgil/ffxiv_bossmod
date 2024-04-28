namespace BossMod.Endwalker.Ultimate.DSW2;

class P6Touchdown(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.TouchdownAOE))
{
    private static readonly AOEShapeCircle _shape = new(20); // TODO: verify falloff

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: activation
        yield return new(_shape, Module.Center);
        yield return new(_shape, Module.Center + new WDir(0, 25));
    }
}

class P6TouchdownCauterize(BossModule module) : BossComponent(module)
{
    private Actor? _nidhogg;
    private Actor? _hraesvelgr;
    private BitMask _boiling;
    private BitMask _freezing;

    private static readonly AOEShapeRect _shape = new(80, 11);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        bool nidhoggSide = actor.Position.X < Module.Center.X; // note: assume nidhogg cleaves whole left side, hraes whole right side
        var forbiddenMask = nidhoggSide ? _boiling : _freezing;
        if (forbiddenMask[slot])
            hints.Add("GTFO from wrong side!");

        // note: assume both dragons are always at north side
        bool isClosest = Raid.WithoutSlot().Where(p => (p.Position.X < Module.Center.X) == nidhoggSide).MinBy(p => p.PosRot.Z) == actor;
        bool shouldBeClosest = actor.Role == Role.Tank;
        if (isClosest != shouldBeClosest)
            hints.Add(shouldBeClosest ? "Move closer to dragons!" : "Move away from dragons!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_boiling[pcSlot] && _nidhogg != null)
            _shape.Draw(Arena, _nidhogg);
        if (_freezing[pcSlot] && _hraesvelgr != null)
            _shape.Draw(Arena, _hraesvelgr);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Boiling:
                _boiling.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.Freezing:
                _freezing.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
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
